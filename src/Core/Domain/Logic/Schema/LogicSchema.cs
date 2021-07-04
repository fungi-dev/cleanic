namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class LogicSchema
    {
        public LanguageSchema Language { get; internal set; }
        public IReadOnlyCollection<AggregateInfo> Aggregates { get; internal set; }
        public IReadOnlyCollection<SagaInfo> Sagas { get; internal set; }
        public IReadOnlyCollection<ServiceInfo> Services { get; internal set; }

        public LogicSchema()
        {
            Aggregates = Array.Empty<AggregateInfo>().ToImmutableHashSet();
            Sagas = Array.Empty<SagaInfo>().ToImmutableHashSet();
            Services = Array.Empty<ServiceInfo>().ToImmutableHashSet();
        }

        public AggregateInfo GetAggregate(EntityInfo entityInfo)
        {
            if (entityInfo == null) throw new ArgumentNullException(nameof(entityInfo));

            var aggregate = Aggregates.SingleOrDefault(x => x.Entity == entityInfo);
            if (aggregate == null) throw new LogicSchemaException($"No aggregate '{entityInfo.Name}' found in domain logic");

            return aggregate;
        }

        public AggregateInfo GetAggregate(CommandInfo commandInfo)
        {
            if (commandInfo == null) throw new ArgumentNullException(nameof(commandInfo));

            var aggregate = Aggregates.SingleOrDefault(x => x.Entity.Commands.Contains(commandInfo));
            if (aggregate == null) throw new LogicSchemaException($"No aggregate with command '{commandInfo.Name}' found in domain logic");

            return aggregate;
        }

        public AggregateInfo GetAggregate(EventInfo eventInfo)
        {
            if (eventInfo == null) throw new ArgumentNullException(nameof(eventInfo));

            var aggregate = Aggregates.SingleOrDefault(x => x.Events.Contains(eventInfo));
            if (aggregate == null) throw new LogicSchemaException($"No aggregate with event '{eventInfo.Name}' found in domain logic");

            return aggregate;
        }

        public EventInfo GetEvent(Type eventType)
        {
            if (eventType == null) throw new ArgumentNullException(nameof(eventType));
            if (!eventType.IsSubclassOf(typeof(Event))) throw new ArgumentOutOfRangeException(nameof(eventType));

            var info = Aggregates.SelectMany(x => x.Events).SingleOrDefault(t => eventType == t.Type);
            return info ?? throw new LogicSchemaException($"No event with type '{eventType.Name}' in logic schema");
        }

        public SagaInfo[] GetReactingSagas(EventInfo eventInfo)
        {
            if (eventInfo == null) throw new ArgumentNullException(nameof(eventInfo));

            return Sagas.Where(x => x.Events.Contains(eventInfo)).ToArray();
        }

        public EventInfo FindEvent(String eventInfoId)
        {
            if (String.IsNullOrEmpty(eventInfoId)) throw new ArgumentNullException(nameof(eventInfoId));

            var events = Aggregates.SelectMany(x => x.Events).Where(x => String.Equals(x.Id, eventInfoId, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (events.Length > 1) throw new LanguageSchemaException($"Many events with ID '{eventInfoId}' found in domain language");
            if (events.Length == 0) throw new LanguageSchemaException($"No events with ID '{eventInfoId}' found in domain language");

            return events.Single();
        }
    }
}