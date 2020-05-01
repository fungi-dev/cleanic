using Cleanic.Application;
using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Framework
{
    public class InMemoryEventStore : IEventStore
    {
        public InMemoryEventStore(DomainMeta domain)
        {
            _domain = domain;
        }

        public Task<Event[]> LoadEvents(AggregateMeta aggregateMeta, String aggregateId)
        {
            var records = Db.Where(x => x.AggregateMeta == aggregateMeta && x.AggregateId == aggregateId);
            return Task.FromResult(records.Select(x => x.Event).ToArray());
        }

        public Task<Event[]> LoadEvents(IEnumerable<EventMeta> eventMetas)
        {
            var records = Db.Where(x => eventMetas.Contains(x.EventMeta));
            return Task.FromResult(records.Select(x => x.Event).ToArray());
        }

        public Task SaveEvents(String aggregateId, UInt32 expectedAggregateVersion, IEnumerable<Event> events)
        {
            var aggregateMeta = events.Select(x => _domain.GetEventMeta(x.GetType()).Aggregate).Distinct().Single();
            var actualAggregateVersion = Db.Where(x => x.AggregateMeta == aggregateMeta && x.AggregateId == aggregateId).Count();
            if (actualAggregateVersion != expectedAggregateVersion) throw new Exception("Concurrent access to event store");
            foreach (var e in events)
            {
                Db.Add(new DataItem
                {
                    AggregateMeta = aggregateMeta,
                    AggregateId = aggregateId,
                    EventMeta = _domain.GetEventMeta(e.GetType()),
                    Event = e
                });
            }
            return Task.CompletedTask;
        }

        private readonly DomainMeta _domain;
        public readonly List<DataItem> Db = new List<DataItem>();

        public class DataItem
        {
            public AggregateMeta AggregateMeta;
            public String AggregateId;
            public EventMeta EventMeta;
            public Event Event;
        }
    }
}