namespace Cleanic.Application
{
    using Cleanic.Core;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventStore
    {
        Task<AggregateEvent[]> LoadEvents(AggregateInfo aggregateInfo, String aggregateId);
        Task<AggregateEvent[]> LoadEvents(IEnumerable<AggregateEventInfo> eventInfos);
        Task SaveEvents(String aggregateId, UInt32 expectedEventsCount, IEnumerable<AggregateEvent> events);
        void ListenEvents(AggregateEventInfo eventInfo, Func<AggregateEvent, Task> listener);
    }
}