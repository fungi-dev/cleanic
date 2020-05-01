using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IEventStore
    {
        Task<Event[]> LoadEvents(AggregateMeta aggregateMeta, String aggregateId);
        Task<Event[]> LoadEvents(IEnumerable<EventMeta> eventMetas);
        Task SaveEvents(String aggregateId, UInt32 expectedAggregateVersion, IEnumerable<Event> aggregateEvents);
    }
}