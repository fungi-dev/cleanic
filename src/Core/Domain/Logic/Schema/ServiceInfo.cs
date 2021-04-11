namespace Cleanic.Core
{
    using System;
    using System.Reflection;

    public class ServiceInfo : DomainObjectInfo
    {
        public ServiceInfo(Type serviceType) : base(serviceType, null)
        {
            if (!serviceType.GetTypeInfo().IsSubclassOf(typeof(Service))) throw new ArgumentOutOfRangeException(nameof(serviceType));
        }
    }
}