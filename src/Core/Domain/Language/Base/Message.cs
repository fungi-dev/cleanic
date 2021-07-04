namespace Cleanic.Core
{
    using System;

    /// <summary>
    /// Data item used in communication between actor of domain and domain itself.
    /// It can be "question" to domain or "answer" given by it.
    /// Always associated with certain entity, can't be "message owned by itself".
    /// </summary>
    public abstract class Message : DomainObject
    {
        /// <summary>
        /// Identifier of entity this message associate to.
        /// </summary>
        public String EntityId { get; set; }

        public override String ToString() => $"{GetType().Name} #{EntityId}";
    }
}