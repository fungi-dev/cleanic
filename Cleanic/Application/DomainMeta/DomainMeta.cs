using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Cleanic.Application
{
    //todo do logging
    /// <summary>
    /// Provides search and traverse functions for domain model.
    /// </summary>
    /// <remarks>
    /// No calls to domain objects, no reflection, no async work.
    /// </remarks>
    public class DomainMeta
    {
        public IReadOnlyCollection<AggregateMeta> Aggregates { get; }
        public IReadOnlyCollection<ServiceMeta> Services { get; }

        public DomainMeta(IEnumerable<DomainObjectMeta> domainObjectMetas)
        {
            domainObjectMetas = domainObjectMetas.ToArray();
            Aggregates = domainObjectMetas.Where(x => x is AggregateMeta).Cast<AggregateMeta>().ToImmutableHashSet();
            Services = domainObjectMetas.Where(x => x is ServiceMeta).Cast<ServiceMeta>().ToImmutableHashSet();
        }

        public CommandMeta GetCommandMeta(Type commandType)
        {
            var meta = Aggregates.SelectMany(x => x.Commands).SingleOrDefault(x => x.Type == commandType);
            if (meta == null) throw PoorDomainException.NoCommand(commandType);
            return meta;
        }

        public EventMeta GetEventMeta(Type eventType)
        {
            var meta = Aggregates.SelectMany(x => x.Events).SingleOrDefault(x => x.Type == eventType);
            if (meta == null) throw PoorDomainException.NoEvent(eventType);
            return meta;
        }

        public EventMeta GetEventMeta(String eventMetaName)
        {
            var meta = Aggregates.SelectMany(x => x.Events).SingleOrDefault(x => String.Equals(x.Name, eventMetaName, StringComparison.OrdinalIgnoreCase));
            if (meta == null) throw PoorDomainException.NoEvent(eventMetaName);
            return meta;
        }

        public ProjectionMeta GetProjectionMeta(Type projectionType)
        {
            var meta = Aggregates.SelectMany(x => x.Projections).SingleOrDefault(x => x.Type == projectionType);
            if (meta == null) throw PoorDomainException.NoProjection(projectionType);
            return meta;
        }

        public AggregateMeta GetAggregateMeta(Type aggregateType)
        {
            var meta = Aggregates.SingleOrDefault(x => x.Type == aggregateType);
            if (meta == null) throw PoorDomainException.NoAggregate(aggregateType);
            return meta;
        }

        public Type FindCommand(String aggregateName, String commandName)
        {
            var aggregate = Aggregates.Single(x => String.Equals(x.Name, aggregateName, StringComparison.OrdinalIgnoreCase));
            return aggregate.Commands.Single(x => String.Equals(x.Name, commandName, StringComparison.OrdinalIgnoreCase)).Type;
        }

        public Type FindQuery(String aggregateName, String projectionName, String queryName)
        {
            var aggregate = Aggregates.Single(x => String.Equals(x.Name, aggregateName, StringComparison.OrdinalIgnoreCase));
            var projection = aggregate.Projections.Single(x => String.Equals(x.Name, projectionName, StringComparison.OrdinalIgnoreCase));
            return projection.Queries.Single(x => String.Equals(x.Name, queryName, StringComparison.OrdinalIgnoreCase)).Type;
        }
    }
}