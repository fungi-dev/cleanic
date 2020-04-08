using System;
using System.Reflection;

namespace Cleanic.Core
{
    public class DomainObjectMeta
    {
        public Type Type { get; }
        public String Name { get; protected set; }

        public DomainObjectMeta(TypeInfo type)
        {
            Type = type.AsType() ?? throw new ArgumentNullException(nameof(type));
            Name = type.Name;
        }

        public override String ToString() => Name;
    }
}