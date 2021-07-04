namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <remarks>
    /// Можно было не создавать отдельный класс для построения схемы, а при создании схемы, чтобы она сама сканила свою ассембли и находила агрегаты/саги/сервисы.
    /// Но это не позволяет запускать приложения, которые должны работать на основе ограниченной части предметки.
    /// А такая ситуация может возникнуть при определении отдельного приложения для построения особо нагруженной вьюхи, например.
    /// Билдер выделен в отдельный класс именно потому, что в подобных ситуациях процесс построения схемы состоит из нескольких шагов.
    /// </remarks>
    public class LogicSchemaBuilder
    {
        public LogicSchemaBuilder(LanguageSchema languageSchema)
        {
            _languageSchema = languageSchema;
        }

        public LogicSchema BuildFromAssembly(Assembly assembly)
        {
            var serviceTypes = assembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(Service)));
            _serviceInfos.AddRange(serviceTypes.Select(x => new ServiceInfo(x)));

            var sagaTypes = assembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(Saga)));
            _sagaInfos.AddRange(sagaTypes.Select(x => new SagaInfo(x)));

            var aggregateTypes = FindAggregateTypesInAssembly(assembly);
            _aggregateInfos.AddRange(aggregateTypes.Select(x => ProduceAggregateInfoFromType(x.Item1, x.Item2)));

            return Build();
        }

        public LogicSchemaBuilder Add<T>()
        {
            var type = typeof(T).GetTypeInfo();

            if (type.IsSubclassOf(typeof(Aggregate)))
            {
                var entityTypeArg = type.BaseType.GenericTypeArguments.Single();
                var entityInfo = _languageSchema.Entities.SingleOrDefault(x => x.Type == entityTypeArg);
                if (entityInfo == null)
                {
                    var m = $"There is no entity in language for aggregate '{type.FullName}'";
                    throw new LogicSchemaException(m);
                }
                _aggregateInfos.Add(ProduceAggregateInfoFromType(entityInfo, type));
            }
            else if (type.IsSubclassOf(typeof(Service)))
            {
                _serviceInfos.Add(new ServiceInfo(type));
            }
            else if (type.IsSubclassOf(typeof(Saga)))
            {
                _sagaInfos.Add(new SagaInfo(type));
            }
            else
            {
                var m = $"Type '{type.FullName}' added in logic schema as an aggregate but it isn't subclass of {nameof(Aggregate)}";
                throw new LogicSchemaException(m);
            }

            return this;
        }

        public LogicSchema Build()
        {
            var servicesRequiredByAggregates = _aggregateInfos.SelectMany(x => x.Dependencies).SelectMany(x => x.Value);
            foreach (var servicesRequiredByAggregate in servicesRequiredByAggregates)
            {
                if (_serviceInfos.Any(x => x.Type == servicesRequiredByAggregate.Type))
                {
                    var m = $"Some aggregate requires service '{servicesRequiredByAggregate.Name}' but is not presented in schema";
                    throw new LogicSchemaException(m);
                }
            }

            foreach (var sagaInfo in _sagaInfos)
            {
                var eventTypes = sagaInfo.Type.GetTypeInfo().DeclaredMethods
                    .SelectMany(m => m.GetParameters())
                    .Select(p => p.ParameterType)
                    .Where(t => t.IsSubclassOf(typeof(Event)))
                    .Distinct();
                var eventInfosFromAggregates = _aggregateInfos.SelectMany(x => x.Events).ToArray();
                var eventInfos = new List<EventInfo>();
                foreach (var eventType in eventTypes)
                {
                    var eventInfo = eventInfosFromAggregates.SingleOrDefault(x => x.Type == eventType);
                    if (eventInfo == null) eventInfo = new EventInfo(eventType);
                    eventInfos.Add(eventInfo);
                }
                sagaInfo.Events = eventInfos.ToImmutableHashSet();
            }

            foreach (var aggregateInfo in _aggregateInfos)
            {
                foreach (var command in aggregateInfo.Entity.Commands)
                {
                    var dependencies = _dependencies.Where(x => x.Key.Type == command.Type);
                    aggregateInfo.Dependencies = dependencies.ToImmutableDictionary(x => x.Key, x => (IReadOnlyCollection<ServiceInfo>)x.Value.Select(t => _serviceInfos.Single(s => s.Type == t)).ToImmutableHashSet());
                }
            }

            return new LogicSchema
            {
                Language = _languageSchema,
                Aggregates = _aggregateInfos.ToImmutableHashSet(),
                Sagas = _sagaInfos.ToImmutableHashSet(),
                Services = _serviceInfos.ToImmutableHashSet()
            };
        }

        private readonly LanguageSchema _languageSchema;
        private readonly List<AggregateInfo> _aggregateInfos = new();
        private readonly List<ServiceInfo> _serviceInfos = new();
        private readonly List<SagaInfo> _sagaInfos = new();
        private readonly Dictionary<CommandInfo, List<Type>> _dependencies = new();

        private IEnumerable<(EntityInfo, Type)> FindAggregateTypesInAssembly(Assembly logicAssembly)
        {
            var aggregateTypes = logicAssembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(Aggregate)));
            foreach (var entityInfo in _languageSchema.Entities)
            {
                var aggregateTypesForOneEntity = aggregateTypes.Where(x => x.BaseType.GenericTypeArguments.Single() == entityInfo.Type);
                foreach (var aggregateTypeForOneEntity in aggregateTypesForOneEntity)
                {
                    yield return (entityInfo, aggregateTypeForOneEntity);
                }
            }
        }

        private AggregateInfo ProduceAggregateInfoFromType(EntityInfo entityInfo, Type aggregateType)
        {
            var aggregateInfo = new AggregateInfo(aggregateType, entityInfo);

            var commandHandlersWithDependencies = aggregateInfo.Type.GetTypeInfo().DeclaredMethods
                .Where(m => m.GetParameters().Length > 1)
                .Where(m => m.GetParameters().Any(p => p.ParameterType.GetTypeInfo().IsSubclassOf(typeof(Command))));
            foreach (var handler in commandHandlersWithDependencies)
            {
                var commandParam = handler.GetParameters().Single(p => p.ParameterType.GetTypeInfo().IsSubclassOf(typeof(Command)));
                var commandInfo = _languageSchema.GetCommand(commandParam.ParameterType);

                var serviceParams = handler.GetParameters().Where(p =>
                {
                    var t = p.ParameterType.GetTypeInfo();
                    if (t.IsArray) t = t.GetElementType().GetTypeInfo();
                    return t.IsSubclassOf(typeof(Service));
                });
                _dependencies.Add(commandInfo, new List<Type>());
                foreach (var serviceType in serviceParams.Select(p => p.ParameterType))
                {
                    var t = serviceType.IsArray ? serviceType.GetElementType() : serviceType;
                    _dependencies[commandInfo].Add(t);
                }
            }

            var eventTypes = aggregateType.GetTypeInfo().DeclaredNestedTypes.Where(x => x.IsSubclassOf(typeof(Event)));
            aggregateInfo.Events = eventTypes.Select(x => new EventInfo(x)).ToImmutableHashSet();

            return aggregateInfo;
        }
    }
}