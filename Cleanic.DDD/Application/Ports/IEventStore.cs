using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IEventStore
    {
        Task<IEvent[]> LoadEvents(AggregateMeta aggregateMeta, IIdentity aggregateId);

        Task<IEvent[]> LoadEvents(IReadOnlyCollection<EventMeta> eventMetas);

        Task SaveEvents(AggregateMeta aggregateMeta, IIdentity aggregateId, IEnumerable<IEvent> events, UInt32 expectedVersion);
    }
}