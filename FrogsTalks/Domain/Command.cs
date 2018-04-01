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
        public Guid AggregateId { get; set; }
    }

    public abstract class Command<T> : Command where T : Aggregate
    {
        public abstract void Run(T aggregate);
    }
}