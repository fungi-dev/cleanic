namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;

    public class Entity : DomainObject
    {
        public String Id { get; }

        public Entity(String id)
        {
            Id = id;
        }

        protected override IEnumerable<Object> GetIdentityComponents()
        {
            yield return Id;
        }
    }
}