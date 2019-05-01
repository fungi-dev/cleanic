using System;

namespace Cleanic.Domain
{
    /// <summary>
    /// Any interaction between system and its user.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The unique identifier of the message subject.
        /// </summary>
        /// <remarks>Subject = Aggregate (for commands and events).</remarks>
        public String SubjectId { get; set; }
    }

    /// <summary>
    /// The request of changes to the domain.
    /// </summary>
    public abstract class Command : Message
    {
        /// <summary>
        /// The unique identifier of the message subject.
        /// </summary>
        /// <remarks>Subject = Aggregate (for commands and events).</remarks>
        public String Id { get; set; }

        public class Result : Message { }

        public abstract class Error : Result { }
    }

    /// <summary>
    /// Represents something that took place in the domain.
    /// </summary>
    /// <remarks>Used for communication between bounded contexts. It is not event from Event Sourcing!</remarks>
    public abstract class Event : Message
    {
        public DateTime Moment { get; set; }

        public override String ToString() => GetType().Name;
    }
}