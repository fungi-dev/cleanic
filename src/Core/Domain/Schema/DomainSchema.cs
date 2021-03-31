namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    public class DomainSchema
    {
        public LanguageSchema Language { get; internal set; }
        public IReadOnlyCollection<AggregateLogicInfo> Aggregates { get; internal set; }
        public IReadOnlyCollection<SagaInfo> Sagas { get; internal set; }
        public IReadOnlyCollection<ServiceInfo> Services { get; internal set; }

        public AggregateLogicInfo GetAggregate(AggregateInfo aggregateInfo)
        {
            if (aggregateInfo == null) throw new ArgumentNullException(nameof(aggregateInfo));

            var info = Aggregates.SingleOrDefault(x => x.Aggregate == aggregateInfo);
            return info ?? throw new DomainSchemaException($"No aggregate logic for {aggregateInfo.Name} in domain");
        }

        public AggregateEventInfo GetAggregateEvent(Type aggregateEventType)
        {
            if (aggregateEventType == null) throw new ArgumentNullException(nameof(aggregateEventType));
            if (!aggregateEventType.GetTypeInfo().IsSubclassOf(typeof(AggregateEvent))) throw new ArgumentOutOfRangeException(nameof(aggregateEventType));

            var info = Aggregates.SelectMany(x => x.Events).SingleOrDefault(t => aggregateEventType == t.Type);
            return info ?? throw new DomainSchemaException($"No aggregate event with type {aggregateEventType.Name} in domain");
        }

        public SagaInfo[] GetReactingSagas(AggregateEventInfo aggregateEventInfo)
        {
            if (aggregateEventInfo == null) throw new ArgumentNullException(nameof(aggregateEventInfo));

            return Sagas.Where(x => x.AggregateEvents.Contains(aggregateEventInfo)).ToArray();
        }

        public Type FindAggregateEvent(String eventFullName)
        {
            if (String.IsNullOrEmpty(eventFullName)) throw new ArgumentNullException(nameof(eventFullName));

            var e = eventFullName.ToLowerInvariant();
            var info = Aggregates.SelectMany(a => a.Events).SingleOrDefault(x => String.Equals(x.FullName, e, StringComparison.OrdinalIgnoreCase));
            return info?.Type ?? throw new LanguageSchemaException($"No event {eventFullName} in domain");
        }
    }
}