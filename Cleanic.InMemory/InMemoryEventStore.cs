using Cleanic.Application;
using Cleanic.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Framework
{
    public class InMemoryEventStore : IEventStore
    {
        public Task<Event[]> LoadEvents(String aggregateName, String aggregateId)
        {
            if (_dictionary.TryGetValue(Tuple.Create(aggregateName, aggregateId), out var list))
                return Task.FromResult(list.ToArray());
            return Task.FromResult(Array.Empty<Event>());
        }

        public Task<Event[]> LoadEvents(IEnumerable<EventMeta> eventMetas)
        {
            var result = new List<Event>();
            foreach (var eventMeta in eventMetas)
            {
                if (_eventsByType.TryGetValue(eventMeta.Name, out var list))
                    result.AddRange(list);
            }

            return Task.FromResult(result.OrderBy(x => x.Moment).ToArray());
        }

        public Task SaveEvents(String aggregateName, String aggregateId, IEnumerable<Event> events, UInt32 expectedVersion)
        {
            var key = Tuple.Create(aggregateName, aggregateId);
            var list = _dictionary.GetOrAdd(key, tuple => new List<Event>(0));

            if (list.Count != expectedVersion) throw new Exception("Concurrent access to event store!");

            list.AddRange(events);
            foreach (var @event in events)
            {
                var eventType = @event.GetType();
                var listOfType = _eventsByType.GetOrAdd(eventType.Name, new List<Event>());
                listOfType.Add(@event);
            }

            return Task.CompletedTask;
        }

        private readonly ConcurrentDictionary<Tuple<String, String>, List<Event>> _dictionary = new ConcurrentDictionary<Tuple<String, String>, List<Event>>();
        private readonly ConcurrentDictionary<String, List<Event>> _eventsByType = new ConcurrentDictionary<String, List<Event>>();
    }
}