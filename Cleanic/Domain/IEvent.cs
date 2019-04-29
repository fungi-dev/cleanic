using System;

namespace Cleanic.Domain
{
    /// <summary>
    /// Represents something that took place in the domain.
    /// </summary>
    public interface IEvent
    {
        String AggregateId { get; set; }
    }
}