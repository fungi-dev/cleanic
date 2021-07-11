namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    public abstract class DomainObjectInfo : IEquatable<DomainObjectInfo>
    {
        protected DomainObjectInfo(Type domainObjectType)
        {
            var guidAttr = domainObjectType.CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof(GuidAttribute));
            if (guidAttr == null) throw new ArgumentException($"Domain object '{domainObjectType.Name}' isn't marked with GuidAttribute", nameof(domainObjectType));
            Id = domainObjectType.GUID.ToString();

            Type = domainObjectType;
            Name = domainObjectType.Name;
        }

        public String Id { get; }
        public String Name { get; protected set; }
        public Type Type { get; }

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

        private static readonly List<DomainObjectInfo> _instances = new();

        protected static void EnsureTermTypeCorrect<T>(Type termType)
        {
            var baseType = typeof(T);
            if (termType == null) throw new ArgumentNullException(nameof(termType));
            if (termType.IsAbstract)
            {
                var m = $"Adding '{termType.FullName}' to schema failed: class should not be abstract";
                throw new LanguageSchemaException(m);
            }
            if (!termType.IsSubclassOf(baseType))
            {
                var m = $"Adding '{termType.FullName}' to schema failed: class should be inherited from '{baseType.FullName}'";
                throw new LanguageSchemaException(m);
            }
        }

        protected static DomainObjectInfo Get(Type domainObjectType, Func<DomainObjectInfo> domainObjectInfoFactory)
        {
            var domainObjectInfo = _instances.SingleOrDefault(i => i.Type == domainObjectType);
            if (domainObjectInfo == null) _instances.Add(domainObjectInfo = domainObjectInfoFactory.Invoke());

            return domainObjectInfo;
        }
    }
}