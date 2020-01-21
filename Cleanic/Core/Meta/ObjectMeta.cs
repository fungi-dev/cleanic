using System;

namespace Cleanic.Core
{
    public class ObjectMeta
    {
        public ObjectMeta(Type objectType)
        {
            Type = objectType ?? throw new ArgumentNullException(nameof(objectType));
            Name = objectType.Name;
        }

        public Type Type { get; }
        public String Name { get; }

        public override String ToString() => Name;
    }
}