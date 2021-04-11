namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;

    public class DomainObjectInfo : IEquatable<DomainObjectInfo>
    {
        public Type Type { get; }
        public String Name { get; protected set; }
        public String FullName { get; }
        public AggregateInfo Aggregate { get; }

        public DomainObjectInfo(Type domainObjectType, AggregateInfo aggregateInfo)
        {
            Type = domainObjectType ?? throw new ArgumentNullException(nameof(domainObjectType));
            Name = domainObjectType.Name;
            FullName = domainObjectType.FullName.Replace("+", ".");
            Aggregate = aggregateInfo;
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