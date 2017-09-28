using System;

namespace FrogsTalks.Domain
{
    /// <summary>
    /// Represents something that took place in the domain.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// The unique identifier of the aggregate.
        /// </summary>
        Guid Id { get; set; }
    }
}