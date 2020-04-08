using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cleanic.Core
{
    public class ServiceMeta : DomainObjectMeta
    {
        public IReadOnlyCollection<EventMeta> Events { get; internal set; }

        public ServiceMeta(TypeInfo serviceType) : base(serviceType) { }

        public Service GetInstance() => (Service)Activator.CreateInstance(Type);

        public Boolean IsHandlingEvent(Type eventType)
        {
            return Type.GetTypeInfo().DeclaredMethods
                .Where(m => !m.IsStatic)
                .Any(m => m.GetParameters().Any(p => p.ParameterType == eventType));
        }
    }
}