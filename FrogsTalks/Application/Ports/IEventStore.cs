using System;
using System.Collections.Generic;
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
        /// <param name="id">Aggregate's identifier.</param>
        /// <returns>All aggregate's events ordered by time.</returns>
        Event[] Load(Guid id);

        /// <summary>
        /// Save events for the aggregate.
        /// </summary>
        /// <param name="id">Aggregate's identifier.</param>
        /// <param name="eventsLoaded">Number of occurred events in the moment when the aggregate was loaded.</param>
        /// <param name="newEvents">The events to be saved.</param>
        void Save(Guid id, Int32 eventsLoaded, Event[] newEvents);
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
        public Event[] Load(Guid id)
        {
            return _db.ContainsKey(id) ? _db[id].ToArray() : Array.Empty<Event>();
        }

        /// <inheritdoc />
        public void Save(Guid id, Int32 eventsLoaded, Event[] newEvents)
        {
            if (!_db.ContainsKey(id)) _db.Add(id, new List<Event>());
            if (_db[id].Count != eventsLoaded) throw new Exception("Concurrency conflict: cannot persist these events!");
            _db[id].AddRange(newEvents);
        }

        private readonly Dictionary<Guid, List<Event>> _db = new Dictionary<Guid, List<Event>>();
    }
}