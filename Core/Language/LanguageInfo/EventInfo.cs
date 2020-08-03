using System;

namespace Cleanic
{
    public class EventInfo : MessageInfo
    {
        public EventInfo(Type eventType, AggregateInfo aggregate) : base(eventType, aggregate) { }
    }
}