namespace Cleanic.Core
{
    using System;

    public sealed class EventInfo : DomainObjectInfo
    {
        public static EventInfo Get(Type type) => (EventInfo)Get(type, () => new EventInfo(type));

        private EventInfo(Type eventType) : base(eventType)
        {
            EnsureTermTypeCorrect<Event>(eventType);
        }
    }
}