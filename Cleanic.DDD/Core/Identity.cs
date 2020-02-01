using System;

namespace Cleanic.Core
{
    public abstract class Identity<T> : ValueObject, IIdentity<T>
        where T : IEntity<T>
    {
        protected Identity(String value)
        {
            Value = value;
        }

        public String Value { get; }

        public override String ToString() => Value;
    }
}