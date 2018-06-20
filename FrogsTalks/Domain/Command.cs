using System;

namespace FrogsTalks.Domain
{
    /// <summary>
    /// The request of changes to the domain.
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// The unique identifier of the aggregate.
        /// </summary>
        public String AggregateId { get; set; }
    }
}