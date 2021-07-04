namespace Cleanic.Core
{
    using System;

    public class EventInfo : DomainObjectInfo
    {
        public EventInfo(Type eventType) : base(eventType)
        {
            EnsureTermTypeCorrect(eventType, typeof(Event));
        }
    }
}