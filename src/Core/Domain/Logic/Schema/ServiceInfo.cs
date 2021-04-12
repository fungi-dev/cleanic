namespace Cleanic.Core
{
    using System;
    using System.Reflection;

    public class ServiceInfo : DomainObjectInfo
    {
        public ServiceInfo(Type serviceType) : base(serviceType)
        {
            if (!serviceType.GetTypeInfo().IsSubclassOf(typeof(Service))) throw new ArgumentOutOfRangeException(nameof(serviceType));
        }
    }
}