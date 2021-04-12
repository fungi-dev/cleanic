namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    public class DomainObjectInfo : IEquatable<DomainObjectInfo>
    {
        public String Id { get; }
        public String Name { get; protected set; }
        public Type Type { get; }

        public DomainObjectInfo(Type domainObjectType)
        {
            if (domainObjectType == null) throw new ArgumentNullException(nameof(domainObjectType));

            var guidAttr = domainObjectType.CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof(GuidAttribute));
            if (guidAttr == null) throw new ArgumentException($"Domain object '{domainObjectType.Name}' isn't marked with GuidAttribute", nameof(domainObjectType));
            Id = domainObjectType.GUID.ToString();

            Type = domainObjectType;
            Name = domainObjectType.Name;
        }

        public override Boolean Equals(Object obj) => Equals(obj as DomainObjectInfo);

        public Boolean Equals(DomainObjectInfo other)
        {
            return other != null && EqualityComparer<Type>.Default.Equals(Type, other.Type);
        }

        public override Int32 GetHashCode()
        {
            return 2049151605 + EqualityComparer<Type>.Default.GetHashCode(Type);
        }

        public override String ToString() => Name;
    }
}