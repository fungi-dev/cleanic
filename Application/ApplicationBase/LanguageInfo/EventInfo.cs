using System;

namespace Cleanic.Application
{
    public class EventInfo : AggregateItemInfo
    {
        public EventInfo(Type eventType, AggregateInfo aggregate) : base(eventType, aggregate) { }
    }
}