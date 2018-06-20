using System;
using System.Collections.Generic;

namespace FrogsTalks.Domain
{
    /// <summary>
    /// The object having identity in the domain.
    /// </summary>
    public abstract class Entity : DomainObject
    {
        protected Entity(String id)
        {
            Id = id;
        }

        /// <summary>
        /// The unique identifier of the entity.
        /// </summary>
        public String Id { get; }

        protected override IEnumerable<Object> GetIdentityComponents()
        {
            yield return Id;
        }
    }
}