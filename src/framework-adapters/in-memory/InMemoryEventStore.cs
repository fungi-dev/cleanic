using Cleanic.Application;
using Cleanic.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Framework
{
    public class InMemoryEventStore : IEventStore
    {
        public InMemoryEventStore(ILogger<InMemoryEventStore> logger, LanguageInfo languageInfo)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Db = new List<DataItem>();
            _bus = new InMemoryEventBus(_logger);
            _languageInfo = languageInfo ?? throw new ArgumentNullException(nameof(languageInfo));
        }

        public List<DataItem> Db { get; }

        public Task<Event[]> LoadEvents(AggregateInfo aggregateInfo, String aggregateId)
        {
            if (aggregateInfo == null) throw new ArgumentNullException(nameof(aggregateInfo));
            if (String.IsNullOrWhiteSpace(aggregateId)) throw new ArgumentNullException(nameof(aggregateId));

            var records = Db.Where(x => x.AggregateInfo == aggregateInfo && x.AggregateId == aggregateId);
            return Task.FromResult(records.Select(x => x.Event).ToArray());
        }

        public Task<Event[]> LoadEvents(IEnumerable<EventInfo> eventInfos)
        {
            eventInfos = eventInfos?.ToArray();
            if (eventInfos == null || !eventInfos.Any()) throw new ArgumentNullException(nameof(eventInfos));

            var records = Db.Where(x => eventInfos.Contains(x.EventInfo));
            return Task.FromResult(records.Select(x => x.Event).ToArray());
        }

        public async Task SaveEvents(String aggregateId, UInt32 expectedEventsCount, IEnumerable<Event> events)
        {
            if (aggregateId == null) throw new ArgumentNullException(nameof(aggregateId));
            events = events?.ToArray();
            if (events == null || !events.Any()) throw new ArgumentNullException(nameof(events));

            var aggregateInfo = events.Select(x => _languageInfo.GetEvent(x.GetType()).Aggregate).Distinct().Single();

            var actualAggregateVersion = Db.Where(x => x.AggregateInfo == aggregateInfo && x.AggregateId == aggregateId).Count();
            if (actualAggregateVersion != expectedEventsCount) throw new Exception("Concurrent access to event store");
            foreach (var @event in events)
            {
                Db.Add(new DataItem
                {
                    AggregateInfo = aggregateInfo,
                    AggregateId = aggregateId,
                    EventInfo = _languageInfo.GetEvent(@event.GetType()),
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

        private readonly ILogger<InMemoryEventStore> _logger;
        private readonly LanguageInfo _languageInfo;
        private readonly InMemoryEventBus _bus;

        public class DataItem
        {
            public AggregateInfo AggregateInfo;
            public String AggregateId;
            public EventInfo EventInfo;
            public Event Event;
        }
    }
}