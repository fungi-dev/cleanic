namespace Cleanic.Application
{
    using Cleanic.Core;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventStore
    {
        Task<Event[]> LoadEvents(EntityInfo entityInfo, String entityId);
        Task<Event[]> LoadEvents(IEnumerable<EventInfo> eventInfos);
        Task SaveEvents(String entityId, UInt32 expectedEventsCount, IEnumerable<Event> events);
        void ListenEvents(EventInfo eventInfo, Func<Event, Task> listener);
    }
}