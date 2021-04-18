namespace Cleanic.Core
{
    using System;

    public abstract class Command : DomainObject
    {
        public String AggregateId { get; set; }

        public override String ToString() => $"{GetType().Name} #{AggregateId}";
    }

    public abstract class InternalCommand : Command { }
}