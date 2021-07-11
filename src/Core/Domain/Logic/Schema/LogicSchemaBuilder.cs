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
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var aggregateTypes = assembly.DefinedTypes.Where(t => t.IsSubclassOf(typeof(Aggregate)));
            foreach (var aggregateType in aggregateTypes) Add(aggregateType);

            var sagaTypes = assembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(Saga)));
            foreach (var sagaType in sagaTypes) Add(sagaType);

            return Build();
        }

        public LogicSchemaBuilder Add<T>() where T : DomainObject
        {
            Add(typeof(T));
            return this;
        }

        public LogicSchemaBuilder Add(Type domainObjectType)
        {
            if (domainObjectType == null) throw new ArgumentNullException(nameof(domainObjectType));

            if (domainObjectType.IsSubclassOf(typeof(Aggregate)))
            {
                var aggregate = AggregateInfo.Get(domainObjectType);
                _aggregateInfos.Add(aggregate);
                _serviceInfos.AddRange(aggregate.Dependencies);
            }
            else if (domainObjectType.IsSubclassOf(typeof(Saga)))
            {
                _sagaInfos.Add(SagaInfo.Get(domainObjectType));
            }
            else
            {
                var m = $"Can't add '{domainObjectType.FullName}' to logic schema, only aggregates and sagas can be added";
                throw new LogicSchemaException(m);
            }

            return this;
        }

        public LogicSchema Build()
        {
            var orphanAggregates = GetAggregatesWithoutEntityInLanguage();
            if (orphanAggregates.Any())
            {
                var m = $"There is no entity in language for aggregates: {String.Join(", ", orphanAggregates)}";
                throw new LogicSchemaException(m);
            }

            var aggregatesWithBadId = GetAggregatesWithIdDifferentFromEntityId();
            if (aggregatesWithBadId.Any())
            {
                var m = $"There is aggregates with identifiers different from their entity identifier: {String.Join(", ", aggregatesWithBadId)}";
                throw new LogicSchemaException(m);
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

        private IEnumerable<AggregateInfo> GetAggregatesWithoutEntityInLanguage() => _aggregateInfos.Where(a => !_languageSchema.Entities.Contains(a.Entity));

        private IEnumerable<AggregateInfo> GetAggregatesWithIdDifferentFromEntityId() => _aggregateInfos.Where(a => a.Id != a.Entity.Id);
    }
}