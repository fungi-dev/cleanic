using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Cleanic.Core
{
    /// <summary>
    /// Helps build application domain from concrete objects specified.
    /// </summary>
    public class DomainMetaBuilder
    {
        public DomainMetaBuilder Aggregate<T>()
        {
            AddDomainTypesFromAssembly(typeof(T).GetTypeInfo().Assembly);

            var containerForAggregateMembers = typeof(T);
            var aggregateType = _domainTypes
                .Where(x => x.IsSubclassOf(typeof(Aggregate)))
                .Single(x => x.BaseType.GenericTypeArguments.Single() == containerForAggregateMembers);
            _aggregates.Add(aggregateType, containerForAggregateMembers.GetTypeInfo());
            _aggregateCommands.Add(aggregateType, new List<TypeInfo>());
            _aggregateEvents.Add(aggregateType, new List<TypeInfo>());
            _aggregateProjections.Add(aggregateType, new List<TypeInfo>());

            var nested = containerForAggregateMembers.GetTypeInfo().DeclaredNestedTypes;

            var commandTypes = nested.Where(x => x.IsSubclassOf(typeof(Command)));
            _aggregateCommands[aggregateType].AddRange(commandTypes);

            var eventTypes = nested.Where(x => x.IsSubclassOf(typeof(Event)));
            _aggregateEvents[aggregateType].AddRange(eventTypes);

            foreach (var projectionType in nested.Where(x => x.IsSubclassOf(typeof(Projection))))
            {
                _aggregateProjections[aggregateType].Add(projectionType);

                _projectionQueries.Add(projectionType, new List<TypeInfo>());
                var queryTypes = projectionType.DeclaredNestedTypes.Where(x => x.IsSubclassOf(typeof(Query)));
                _projectionQueries[projectionType].AddRange(queryTypes);

                var builder = _domainTypes
                    .Where(x => x.IsSubclassOf(typeof(ProjectionBuilder)))
                    .Single(x => x.BaseType.GenericTypeArguments.Single() == projectionType.AsType());
                _projectionBuilders.Add(projectionType, builder);
            }

            return this;
        }

        public DomainMetaBuilder Service<T>()
            where T : Service
        {
            AddDomainTypesFromAssembly(typeof(T).GetTypeInfo().Assembly);

            var serviceType = typeof(T).GetTypeInfo();
            _services.Add(serviceType);
            return this;
        }

        public DomainMeta Build()
        {
            var aggregates = new List<AggregateMeta>();
            foreach (var agg in _aggregates)
            {
                var aggMeta = new AggregateMeta(agg.Key, agg.Value);

                aggMeta.Commands = _aggregateCommands[agg.Key].Select(x => new CommandMeta(x, aggMeta)).ToImmutableHashSet();
                aggMeta.Events = _aggregateEvents[agg.Key].Select(x => new EventMeta(x, aggMeta)).ToImmutableHashSet();
                var projections = new List<ProjectionMeta>();
                foreach (var projection in _aggregateProjections[agg.Key])
                {
                    var prjMeta = new ProjectionMeta(projection, _projectionBuilders[projection], aggMeta);
                    prjMeta.Queries = _projectionQueries[projection].Select(x => new QueryMeta(x, prjMeta)).ToImmutableHashSet();
                    projections.Add(prjMeta);
                }
                aggMeta.Projections = projections.ToImmutableHashSet();

                aggregates.Add(aggMeta);
            }
            foreach (var prjMeta in aggregates.SelectMany(a => a.Projections))
            {
                var domainEvents = aggregates.SelectMany(x => x.Events);
                var prjEvents = domainEvents.Where(x => prjMeta.IsHandlingEvent(x.Type));
                prjMeta.Events = prjEvents.ToImmutableHashSet();
            }

            var services = new List<ServiceMeta>();
            foreach (var svcType in _services)
            {
                var svcMeta = new ServiceMeta(svcType);
                var domainEvents = aggregates.SelectMany(x => x.Events);
                var svcEvents = domainEvents.Where(x => svcMeta.IsHandlingEvent(x.Type));
                svcMeta.Events = svcEvents.ToImmutableHashSet();
                services.Add(svcMeta);
            }

            foreach (var agg in aggregates) agg.InjectServices(services);

            return new DomainMeta(services.Cast<DomainObjectMeta>().Concat(aggregates));
        }

        private void AddDomainTypesFromAssembly(Assembly assembly)
        {
            foreach (var t in assembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(DomainObject))))
            {
                _domainTypes.Add(t);
            }
        }

        private readonly HashSet<TypeInfo> _domainTypes = new HashSet<TypeInfo>();
        private readonly Dictionary<TypeInfo, TypeInfo> _aggregates = new Dictionary<TypeInfo, TypeInfo>();
        private readonly Dictionary<TypeInfo, List<TypeInfo>> _aggregateCommands = new Dictionary<TypeInfo, List<TypeInfo>>();
        private readonly Dictionary<TypeInfo, List<TypeInfo>> _aggregateEvents = new Dictionary<TypeInfo, List<TypeInfo>>();
        private readonly Dictionary<TypeInfo, List<TypeInfo>> _aggregateProjections = new Dictionary<TypeInfo, List<TypeInfo>>();
        private readonly Dictionary<TypeInfo, List<TypeInfo>> _projectionQueries = new Dictionary<TypeInfo, List<TypeInfo>>();
        private readonly Dictionary<TypeInfo, TypeInfo> _projectionBuilders = new Dictionary<TypeInfo, TypeInfo>();
        private readonly List<TypeInfo> _services = new List<TypeInfo>();
    }
}