using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrogsTalks.Domain;

namespace FrogsTalks.Application.Ports
{
    /// <summary>
    /// An abstract storage of events.
    /// </summary>
    /// <remarks>One of the application layer ports needed to have adapter in outer layer.</remarks>
    public interface IEventStore
    {
        /// <summary>
        /// Load all events for the aggregate.
        /// </summary>
        /// <param name="aggregateId">Aggregate's identifier.</param>
        /// <returns>All aggregate's events ordered by time.</returns>
        Task<Event[]> Load(String aggregateId);

        /// <summary>
        /// Save events for the aggregate.
        /// </summary>
        /// <param name="aggregateId">Aggregate's identifier.</param>
        /// <param name="lastVersion">Number of occurred events in the moment when the aggregate was loaded.</param>
        /// <param name="newEvents">The events to be saved.</param>
        Task Save(String aggregateId, UInt32 lastVersion, Event[] newEvents);

        Task Clear();
    }

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