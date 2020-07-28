using System;

namespace Cleanic
{
    public class EventInfo : AggregateItemInfo
    {
        public EventInfo(Type eventType, AggregateInfo aggregate) : base(eventType, aggregate) { }
    }
}