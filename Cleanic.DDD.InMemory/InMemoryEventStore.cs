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
        public Task<IEvent[]> LoadEvents(AggregateMeta aggregateMeta, IIdentity aggregateId)
        {
            if (_dictionary.TryGetValue(Tuple.Create(aggregateMeta.Type, aggregateId), out var list))
                return Task.FromResult(list.ToArray());
            return Task.FromResult(Array.Empty<IEvent>());
        }

        public Task<IEvent[]> LoadEvents(IReadOnlyCollection<EventMeta> eventMetas)
        {
            var result = new List<IEvent>();
            foreach (var eventMeta in eventMetas)
            {
                if (_eventsByType.TryGetValue(eventMeta.Type, out var list))
                    result.AddRange(list);
            }

            return Task.FromResult(result.OrderBy(x => x.Moment).ToArray());
        }

        public Task SaveEvents(AggregateMeta aggregateMeta, IIdentity aggregateId, IEnumerable<IEvent> events, UInt32 expectedVersion)
        {
            var key = Tuple.Create(aggregateMeta.Type, aggregateId);
            var list = _dictionary.GetOrAdd(key, tuple => new List<IEvent>(0));

            if (list.Count != expectedVersion) throw new Exception("Concurrent access to event store!");

            list.AddRange(events);
            foreach (var @event in events)
            {
                var eventType = @event.GetType();
                var listOfType = _eventsByType.GetOrAdd(eventType, new List<IEvent>());
                listOfType.Add(@event);
            }

            return Task.CompletedTask;
        }

        private readonly ConcurrentDictionary<Tuple<Type, IIdentity>, List<IEvent>> _dictionary = new ConcurrentDictionary<Tuple<Type, IIdentity>, List<IEvent>>();
        private readonly ConcurrentDictionary<Type, List<IEvent>> _eventsByType = new ConcurrentDictionary<Type, List<IEvent>>();
    }
}