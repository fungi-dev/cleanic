namespace Cleanic.Core
{
    using System;

    public class Message : DomainObject
    {
        public String AggregateId { get; set; }

        public override String ToString() => $"{GetType().Name} #{AggregateId}";
    }
}