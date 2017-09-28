using FrogsTalks.Domain;
using System;
using System.Collections.Generic;

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
        IEvent[] Load(Guid id);

        /// <summary>
        /// Save events for the aggregate.
        /// </summary>
        /// <param name="id">Aggregate's identifier.</param>
        /// <param name="eventsLoaded">Number of occurred events in the moment when the aggregate was loaded.</param>
        /// <param name="newEvents">The events to be saved.</param>
        void Save(Guid id, int eventsLoaded, IEvent[] newEvents);
    }

    /// <summary>
    /// Event store working in memory.
    /// </summary>
    /// <remarks>
    /// This is an embedded <see cref="IEventStore">port</see> adapter implementation for tests and experiments.
    /// </remarks>
    public class InMemoryEventStore : IEventStore
    {
        /// <summary>
        /// Load all events for the aggregate.
        /// </summary>
        /// <param name="id">Aggregate's identifier.</param>
        /// <returns>All aggregate's events ordered by time.</returns>
        public IEvent[] Load(Guid id)
        {
            if (db.ContainsKey(id)) return db[id].ToArray();
            return Array.Empty<IEvent>();
        }

        /// <summary>
        /// Save events for the aggregate.
        /// </summary>
        /// <param name="id">Aggregate's identifier.</param>
        /// <param name="eventsLoaded">Number of occurred events in the moment when the aggregate was loaded.</param>
        /// <param name="newEvents">The events to be saved.</param>
        public void Save(Guid id, int eventsLoaded, IEvent[] newEvents)
        {
            if (!db.ContainsKey(id)) db.Add(id, new List<IEvent>());
            if (db[id].Count != eventsLoaded) throw new Exception("Concurrency conflict: cannot persist these events!");
            db[id].AddRange(newEvents);
        }

        private Dictionary<Guid, List<IEvent>> db = new Dictionary<Guid, List<IEvent>>();
    }
}