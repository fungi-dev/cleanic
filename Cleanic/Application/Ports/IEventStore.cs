using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IEventStore
    {
        Task<Event[]> LoadEvents(String aggregateName, String aggregateId);
        Task<Event[]> LoadEvents(IEnumerable<EventMeta> eventMetas);
        Task SaveEvents(String aggregateName, String aggregateId, IEnumerable<Event> events, UInt32 expectedVersion);
    }
}