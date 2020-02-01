using System;
using System.Collections.Generic;

namespace Cleanic.Core
{
    public abstract class Entity<T> : DomainObject, IEntity<T>
        where T : IEntity<T>
    {
        protected Entity(IIdentity<T> id)
        {
            Id = id;
        }

        public IIdentity<T> Id { get; }

        IIdentity IEntity.Id => Id;

        protected override IEnumerable<Object> GetIdentityComponents()
        {
            yield return Id;
        }
    }
}