namespace Cleanic.Core
{
    using System;

    public abstract class Event : DomainObject
    {
        public String EntityId { get; set; }
        public DateTime EventOccurred { get; set; }

        public override String ToString() => $"{GetType().Name} #{EntityId}";
    }
}