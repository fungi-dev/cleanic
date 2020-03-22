using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IEventStore
    {
        Task<IEvent[]> LoadEvents(String aggregateName, String aggregateId);
        Task<IEvent[]> LoadEvents(IEnumerable<EventMeta> eventMetas);
        Task SaveEvents(String aggregateName, String aggregateId, IEnumerable<IEvent> events, UInt32 expectedVersion);
    }
}