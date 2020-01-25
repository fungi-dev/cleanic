using Cleanic.Application;
using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cleanic.Framework
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
        public Task<IEvent[]> Load(IIdentity aggregateId)
        {
            return Task.FromResult(_db.ContainsKey(aggregateId) ? _db[aggregateId].ToArray() : Array.Empty<IEvent>());
        }

        /// <inheritdoc />
        public Task Save(IIdentity aggregateId, UInt32 lastVersion, IEnumerable<IEvent> newEvents)
        {
            if (!_db.ContainsKey(aggregateId)) _db.Add(aggregateId, new List<IEvent>());
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

        private readonly Dictionary<IIdentity, List<IEvent>> _db = new Dictionary<IIdentity, List<IEvent>>();
    }
}