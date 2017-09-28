using System;

namespace FrogsTalks.Domain
{
    /// <summary>
    /// The object having identity in the domain.
    /// </summary>
    public class Entity : DomainObject
    {
        /// <summary>
        /// The unique identifier of the entity.
        /// </summary>
        public Guid Id { get; set; }
    }
}