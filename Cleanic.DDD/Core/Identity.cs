using System;

namespace Cleanic.Core
{
    public abstract class Identity<T> : ValueObject, IIdentity<T>
        where T : Entity<T>
    {
        protected Identity(String value)
        {
            Value = value;
        }

        public String Value { get; }
    }
}