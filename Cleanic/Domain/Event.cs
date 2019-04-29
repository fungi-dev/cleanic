using System;

namespace Cleanic.Domain
{
    /// <summary>
    /// Represents something that took place in the domain.
    /// </summary>
    public abstract class Event : ValueObject
    {
        /// <summary>
        /// The unique identifier of the aggregate.
        /// </summary>
        public String AggregateId { get; set; }

        public DateTime Moment { get; set; }

        public override String ToString()
        {
            return GetType().Name;
        }
    }

    public abstract class FirstEvent : Event { }
}