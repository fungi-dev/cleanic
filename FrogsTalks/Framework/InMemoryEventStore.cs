using FrogsTalks.Application;
using FrogsTalks.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrogsTalks.Framework
{
    /// <summary>
    /// Event store working in memory.
    /// </summary>
    /// <remarks>
    /// This is an embedded <see cref="IEventStore">port</see> adapter implementation for tests and experiments.
    /// </remarks>
    public sealed class InMemoryEventStore : IEventStore
    {
        /// <inheritdoc />
        public Task<Event[]> Load(String aggregateId)
        {
            return Task.FromResult(_db.ContainsKey(aggregateId) ? _db[aggregateId].ToArray() : Array.Empty<Event>());
        }

        /// <inheritdoc />
        public Task Save(String aggregateId, UInt32 lastVersion, Event[] newEvents)
        {
            if (!_db.ContainsKey(aggregateId)) _db.Add(aggregateId, new List<Event>());
            var savedVersion = Convert.ToUInt32(_db[aggregateId].Count);
            if (savedVersion != lastVersion) throw new Exception("Concurrency conflict: cannot persist these events!");
            _db[aggregateId].AddRange(newEvents);

            return Task.CompletedTask;
        }

        public Task Clear()
        {
            _db.Clear();
            return Task.CompletedTask;
        }

        private readonly Dictionary<String, List<Event>> _db = new Dictionary<String, List<Event>>();
    }
}