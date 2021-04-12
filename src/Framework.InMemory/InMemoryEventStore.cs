namespace Cleanic.Framework
{
    using Cleanic.Application;
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class InMemoryEventStore : IEventStore
    {
        public InMemoryEventStore(LogicSchema logicSchema, ILogger<InMemoryEventStore> logger)
        {
            _logicSchema = logicSchema ?? throw new ArgumentNullException(nameof(logicSchema));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Db = new List<DataItem>();
            _bus = new InMemoryEventBus(logger);
        }

        public List<DataItem> Db { get; }

        public Task<AggregateEvent[]> LoadEvents(AggregateInfo aggregateInfo, String aggregateId)
        {
            if (aggregateInfo == null) throw new ArgumentNullException(nameof(aggregateInfo));
            if (String.IsNullOrWhiteSpace(aggregateId)) throw new ArgumentNullException(nameof(aggregateId));

            var records = Db.Where(x => x.AggregateInfo == aggregateInfo && x.AggregateId == aggregateId);
            return Task.FromResult(records.Select(x => x.Event).ToArray());
        }

        public Task<AggregateEvent[]> LoadEvents(IEnumerable<AggregateEventInfo> eventInfos)
        {
            eventInfos = eventInfos?.ToArray();
            if (eventInfos == null || !eventInfos.Any()) throw new ArgumentNullException(nameof(eventInfos));

            var records = Db.Where(x => eventInfos.Contains(x.EventInfo));
            return Task.FromResult(records.Select(x => x.Event).ToArray());
        }

        public async Task SaveEvents(String aggregateId, UInt32 expectedEventsCount, IEnumerable<AggregateEvent> events)
        {
            if (aggregateId == null) throw new ArgumentNullException(nameof(aggregateId));
            events = events?.ToArray();
            if (events == null || !events.Any()) throw new ArgumentNullException(nameof(events));

            var eventInfos = events.Select(x => _logicSchema.GetAggregateEvent(x.GetType()));
            var aggregateLogicInfos = eventInfos.Select(x => _logicSchema.GetAggregate(x).AggregateFromLanguage);
            var aggregateInfo = aggregateLogicInfos.Distinct().Single();

            var actualAggregateVersion = Db.Where(x => x.AggregateInfo == aggregateInfo && x.AggregateId == aggregateId).Count();
            if (actualAggregateVersion != expectedEventsCount) throw new Exception("Concurrent access to event store");
            foreach (var @event in events)
            {
                Db.Add(new DataItem
                {
                    AggregateInfo = aggregateInfo,
                    AggregateId = aggregateId,
                    EventInfo = _logicSchema.GetAggregateEvent(@event.GetType()),
                    Event = @event
                });
                await _bus.Publish(@event);
            }
        }

        public void ListenEvents(AggregateEventInfo eventInfo, Func<AggregateEvent, Task> listener)
        {
            if (eventInfo == null) throw new ArgumentNullException(nameof(eventInfo));
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            _bus.ListenEvents(eventInfo.Type, listener);
        }

        private readonly LogicSchema _logicSchema;
        private readonly ILogger _logger;
        private readonly InMemoryEventBus _bus;

        public class DataItem
        {
            public AggregateInfo AggregateInfo;
            public String AggregateId;
            public AggregateEventInfo EventInfo;
            public AggregateEvent Event;
        }
    }
}