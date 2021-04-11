namespace Cleanic.Core
{
    using System;

    public abstract class Command : DomainObject
    {
        public String AggregateId { get; set; }
    }

    public abstract class InternalCommand : Command { }
}