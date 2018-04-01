using System;
using System.Collections.Generic;
using System.Linq;

namespace FrogsTalks.Domain
{
    /// <summary>
    /// The object having meaning in the domain.
    /// </summary>
    public abstract class DomainObject : IEquatable<DomainObject>
    {
        /// <summary>
        /// When overriden in a derived class, returns all components which constitute identity.
        /// </summary>
        /// <returns>An ordered list of identity components.</returns>
        protected abstract IEnumerable<Object> GetIdentityComponents();

        public Boolean Equals(DomainObject other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            var components = GetIdentityComponents();
            var otherComponents = other.GetIdentityComponents();
            return components.SequenceEqual(otherComponents);
        }

        public override Boolean Equals(Object other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != GetType()) return false;
            return Equals((DomainObject)other);
        }

        public override Int32 GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                foreach (var component in GetIdentityComponents())
                {
                    hash = hash * 23 + component?.GetHashCode() ?? 0;
                }
                return hash;
            }
        }

        public static Boolean operator ==(DomainObject left, DomainObject right)
        {
            return Equals(left, right);
        }

        public static Boolean operator !=(DomainObject left, DomainObject right)
        {
            return !Equals(left, right);
        }
    }
}