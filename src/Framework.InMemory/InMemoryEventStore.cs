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

        public Task<Event[]> LoadEvents(EntityInfo entityInfo, String entityId)
        {
            if (entityInfo == null) throw new ArgumentNullException(nameof(entityInfo));
            if (String.IsNullOrWhiteSpace(entityId)) throw new ArgumentNullException(nameof(entityId));

            var records = Db.Where(x => x.EntityInfo == entityInfo && x.EntityId == entityId);
            return Task.FromResult(records.Select(x => x.Event).ToArray());
        }

        public Task<Event[]> LoadEvents(IEnumerable<EventInfo> eventInfos)
        {
            eventInfos = eventInfos?.ToArray();
            if (eventInfos == null || !eventInfos.Any()) throw new ArgumentNullException(nameof(eventInfos));

            var records = Db.Where(x => eventInfos.Contains(x.EventInfo));
            return Task.FromResult(records.Select(x => x.Event).ToArray());
        }

        public async Task SaveEvents(String entityId, UInt32 expectedEventsCount, IEnumerable<Event> events)
        {
            if (entityId == null) throw new ArgumentNullException(nameof(entityId));
            events = events?.ToArray();
            if (events == null || !events.Any()) throw new ArgumentNullException(nameof(events));

            var eventInfos = events.Select(x => _logicSchema.GetEvent(x.GetType()));
            var entityInfos = eventInfos.Select(x => _logicSchema.GetAggregate(x).Entity);
            var entityInfo = entityInfos.Distinct().Single();

            var actualAggregateVersion = Db.Where(x => x.EntityInfo == entityInfo && x.EntityId == entityId).Count();
            if (actualAggregateVersion != expectedEventsCount) throw new Exception("Concurrent access to event store");
            foreach (var @event in events)
            {
                Db.Add(new DataItem
                {
                    EntityInfo = entityInfo,
                    EntityId = entityId,
                    EventInfo = _logicSchema.GetEvent(@event.GetType()),
                    Event = @event
                });
                await _bus.Publish(@event);
            }
        }

        public void ListenEvents(EventInfo eventInfo, Func<Event, Task> listener)
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
            public EntityInfo EntityInfo;
            public String EntityId;
            public EventInfo EventInfo;
            public Event Event;
        }
    }
}