namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    public class LogicSchema
    {
        public LanguageSchema Language { get; internal set; }
        public IReadOnlyCollection<AggregateLogicInfo> Aggregates { get; internal set; }
        public IReadOnlyCollection<SagaInfo> Sagas { get; internal set; }
        public IReadOnlyCollection<ServiceInfo> Services { get; internal set; }

        public AggregateLogicInfo GetAggregate(AggregateInfo aggregateInfo)
        {
            if (aggregateInfo == null) throw new ArgumentNullException(nameof(aggregateInfo));

            var aggregate = Aggregates.SingleOrDefault(x => x.AggregateFromLanguage == aggregateInfo);
            if (aggregate == null) throw new LogicSchemaException($"No aggregate '{aggregateInfo.Name}' found in domain logic");

            return aggregate;
        }

        public AggregateLogicInfo GetAggregate(CommandInfo commandInfo)
        {
            if (commandInfo == null) throw new ArgumentNullException(nameof(commandInfo));

            var aggregate = Aggregates.SingleOrDefault(x => x.AggregateFromLanguage.Commands.Contains(commandInfo));
            if (aggregate == null) throw new LogicSchemaException($"No aggregate with command '{commandInfo.Name}' found in domain logic");

            return aggregate;
        }

        public AggregateLogicInfo GetAggregate(AggregateEventInfo aggregateEventInfo)
        {
            if (aggregateEventInfo == null) throw new ArgumentNullException(nameof(aggregateEventInfo));

            var aggregate = Aggregates.SingleOrDefault(x => x.Events.Contains(aggregateEventInfo));
            if (aggregate == null) throw new LogicSchemaException($"No aggregate with event '{aggregateEventInfo.Name}' found in domain logic");

            return aggregate;
        }

        public AggregateEventInfo GetAggregateEvent(Type aggregateEventType)
        {
            if (aggregateEventType == null) throw new ArgumentNullException(nameof(aggregateEventType));
            if (!aggregateEventType.GetTypeInfo().IsSubclassOf(typeof(AggregateEvent))) throw new ArgumentOutOfRangeException(nameof(aggregateEventType));

            var info = Aggregates.SelectMany(x => x.Events).SingleOrDefault(t => aggregateEventType == t.Type);
            return info ?? throw new LogicSchemaException($"No aggregate event with type '{aggregateEventType.Name}' in logic schema");
        }

        public SagaInfo[] GetReactingSagas(AggregateEventInfo aggregateEventInfo)
        {
            if (aggregateEventInfo == null) throw new ArgumentNullException(nameof(aggregateEventInfo));

            return Sagas.Where(x => x.AggregateEvents.Contains(aggregateEventInfo)).ToArray();
        }

        public AggregateEventInfo FindAggregateEvent(String eventInfoId)
        {
            if (String.IsNullOrEmpty(eventInfoId)) throw new ArgumentNullException(nameof(eventInfoId));

            var events = Aggregates.SelectMany(x => x.Events).Where(x => String.Equals(x.Id, eventInfoId, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (events.Length > 1) throw new LanguageSchemaException($"Many aggregate events with ID '{eventInfoId}' found in domain language");
            if (events.Length == 0) throw new LanguageSchemaException($"No aggregate events with ID '{eventInfoId}' found in domain language");

            return events.Single();
        }
    }
}