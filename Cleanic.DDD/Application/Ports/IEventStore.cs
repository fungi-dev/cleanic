using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cleanic.Application
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
        Task<IEvent[]> Load(IIdentity aggregateId);

        /// <summary>
        /// Save events for the aggregate.
        /// </summary>
        /// <param name="aggregateId">Aggregate's identifier.</param>
        /// <param name="lastVersion">Number of occurred events in the moment when the aggregate was loaded.</param>
        /// <param name="newEvents">The events to be saved.</param>
        Task Save(IIdentity aggregateId, UInt32 lastVersion, IEnumerable<IEvent> newEvents);

        Task Clear();
    }
}