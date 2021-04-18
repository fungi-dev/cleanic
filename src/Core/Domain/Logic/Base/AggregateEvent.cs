namespace Cleanic.Core
{
    using System;

    public abstract class AggregateEvent : DomainObject
    {
        public String AggregateId { get; set; }
        public DateTime EventOccurred { get; set; }

        public override String ToString() => $"{GetType().Name} #{AggregateId}";
    }
}