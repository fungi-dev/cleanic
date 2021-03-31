namespace Cleanic.Core
{
    using System;

    public class DomainObjectInfo
    {
        public Type Type { get; }
        public String Name { get; protected set; }

        public DomainObjectInfo(Type domainObjectType)
        {
            Type = domainObjectType ?? throw new ArgumentNullException(nameof(domainObjectType));
            Name = domainObjectType.FullName.Replace("+", ".");
        }

        public override String ToString() => Name;
    }
}