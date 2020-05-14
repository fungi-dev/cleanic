using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IEventStore
    {
        Task<Event[]> LoadEvents(AggregateInfo aggregateInfo, String aggregateId);
        Task<Event[]> LoadEvents(IEnumerable<EventInfo> eventInfos);
        Task SaveEvents(String aggregateId, UInt32 expectedEventsCount, IEnumerable<Event> events);
        void ListenEvents(EventInfo eventInfo, Func<Event, Task> listener);
    }
}