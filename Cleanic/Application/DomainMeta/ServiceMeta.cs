using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cleanic.Application
{
    public class ServiceMeta : DomainObjectMeta
    {
        public IReadOnlyCollection<EventMeta> Events { get; internal set; }

        public ServiceMeta(TypeInfo serviceType) : base(serviceType) { }

        public Boolean IsHandlingEvent(Type eventType)
        {
            return Type.GetTypeInfo().DeclaredMethods
                .Where(m => !m.IsStatic)
                .Any(m => m.GetParameters().Any(p => p.ParameterType == eventType));
        }
    }
}