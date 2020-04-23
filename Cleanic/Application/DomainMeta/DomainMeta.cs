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
            return Aggregates.SelectMany(x => x.Commands).Single(x => x.Type == commandType);
        }

        public EventMeta GetEventMeta(Type eventType)
        {
            return Aggregates.SelectMany(x => x.Events).Single(x => x.Type == eventType);
        }

        public ProjectionMeta GetProjectionMeta(Type projectionType)
        {
            return Aggregates.SelectMany(x => x.Projections).Single(x => x.Type == projectionType);
        }

        public AggregateMeta GetAggregateMeta(Type aggregateType)
        {
            return Aggregates.Single(x => x.Type == aggregateType);
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