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

            var aggregateLogicTypes = FindAggregateLogicTypesInAssembly(assembly);
            _aggregateLogicInfos.AddRange(aggregateLogicTypes.Select(x => ProduceAggregateLogicInfoFromType(x.Item1, x.Item2)));

            return Build();
        }

        public LogicSchemaBuilder Add<T>()
        {
            var type = typeof(T).GetTypeInfo();

            if (type.IsSubclassOf(typeof(Aggregate)))
            {
                var aggTypeArg = type.BaseType.GenericTypeArguments.Single();
                var aggregateInfo = _languageSchema.Aggregates.SingleOrDefault(x => x.Type == aggTypeArg);
                if (aggregateInfo == null)
                {
                    var m = $"There is no aggregate in language for aggregate logic '{type.FullName}'";
                    throw new LogicSchemaException(m);
                }
                _aggregateLogicInfos.Add(ProduceAggregateLogicInfoFromType(aggregateInfo, type));
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
                var m = $"Type '{type.FullName}' added in logic schema as an aggregate but it isn't subclass of AggregateLogic";
                throw new LogicSchemaException(m);
            }

            return this;
        }

        public LogicSchema Build()
        {
            var servicesRequiredByAggregates = _aggregateLogicInfos.SelectMany(x => x.Dependencies).SelectMany(x => x.Value);
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
                var events = sagaInfo.Type.GetTypeInfo().DeclaredMethods
                    .SelectMany(m => m.GetParameters())
                    .Select(p => p.ParameterType)
                    .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(AggregateEvent)))
                    .Distinct()
                    .Select(t => _aggregateLogicInfos.SelectMany(x => x.Events).Single(x => x.Type == t));
                sagaInfo.AggregateEvents = events.ToImmutableHashSet();
            }

            foreach (var aggregateLogicInfo in _aggregateLogicInfos)
            {
                foreach (var command in aggregateLogicInfo.AggregateFromLanguage.Commands)
                {
                    var dependencies = _dependencies.Where(x => x.Key.Type == command.Type);
                    aggregateLogicInfo.Dependencies = dependencies.ToImmutableDictionary(x => x.Key, x => (IReadOnlyCollection<ServiceInfo>)x.Value.Select(t => _serviceInfos.Single(s => s.Type == t)).ToImmutableHashSet());
                }
            }

            return new LogicSchema
            {
                Language = _languageSchema,
                Aggregates = _aggregateLogicInfos.ToImmutableHashSet(),
                Sagas = _sagaInfos.ToImmutableHashSet(),
                Services = _serviceInfos.ToImmutableHashSet()
            };
        }

        private readonly LanguageSchema _languageSchema;
        private readonly List<AggregateLogicInfo> _aggregateLogicInfos = new List<AggregateLogicInfo>();
        private readonly List<ServiceInfo> _serviceInfos = new List<ServiceInfo>();
        private readonly List<SagaInfo> _sagaInfos = new List<SagaInfo>();
        private readonly Dictionary<CommandInfo, List<Type>> _dependencies = new Dictionary<CommandInfo, List<Type>>();

        private IEnumerable<(AggregateInfo, Type)> FindAggregateLogicTypesInAssembly(Assembly logicAssembly)
        {
            var aggregateLogicTypes = logicAssembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(AggregateLogic<>)));
            foreach (var aggregateInfo in _languageSchema.Aggregates)
            {
                var aggregateLogicTypesForOneAggregate = aggregateLogicTypes.Where(x => x.GenericTypeArguments.Single() == aggregateInfo.Type);
                foreach (var aggregateLogicTypeForOneAggregate in aggregateLogicTypesForOneAggregate)
                {
                    yield return (aggregateInfo, aggregateLogicTypeForOneAggregate);
                }
            }
        }

        private AggregateLogicInfo ProduceAggregateLogicInfoFromType(AggregateInfo aggregateInfo, Type aggregateLogicType)
        {
            var aggregateLogicInfo = new AggregateLogicInfo(aggregateLogicType, aggregateInfo);

            var commandHandlersWithDependencies = aggregateLogicInfo.Type.GetTypeInfo().DeclaredMethods
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

            var eventTypes = aggregateLogicType.GetTypeInfo().DeclaredNestedTypes.Where(x => x.IsSubclassOf(typeof(AggregateEvent)));
            aggregateLogicInfo.Events = eventTypes.Select(x => new AggregateEventInfo(x)).ToImmutableHashSet();

            return aggregateLogicInfo;
        }
    }
}