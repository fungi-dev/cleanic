namespace Cleanic.Core
{
    using System;

    public class ServiceInfo : DomainObjectInfo
    {
        public ServiceInfo(Type serviceType) : base(serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            var baseType = typeof(Service);
            if (!serviceType.IsSubclassOf(baseType))
            {
                var m = $"Adding '{serviceType.FullName}' to language schema failed: class should be inherited from '{baseType.FullName}'";
                throw new LanguageSchemaException(m);
            }
        }
    }
}