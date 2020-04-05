using System;

namespace Cleanic.Core
{
    public class Id<T> : ValueObject, IIdentity<T>
        where T : IEntity<T>
    {
        public Id(String value)
        {
            Value = value;
        }

        public String Value { get; }

        public override String ToString() => Value;
    }
}