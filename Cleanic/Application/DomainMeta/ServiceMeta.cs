using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cleanic.Core
{
    public class ServiceMeta : DomainObjectMeta
    {
        public IReadOnlyCollection<EventMeta> Events { get; internal set; }

        public ServiceMeta(TypeInfo serviceType, Func<Service> serviceFactory) : base(serviceType)
        {
            _factory = serviceFactory;
        }

        public Service GetInstance() => _factory.Invoke();

        public Boolean IsHandlingEvent(Type eventType)
        {
            return Type.GetTypeInfo().DeclaredMethods
                .Where(m => !m.IsStatic)
                .Any(m => m.GetParameters().Any(p => p.ParameterType == eventType));
        }

        private readonly Func<Service> _factory;
    }
}