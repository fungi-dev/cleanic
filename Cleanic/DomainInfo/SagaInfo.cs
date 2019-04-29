using Cleanic.Domain;
using System;
using System.Linq;
using System.Reflection;

namespace Cleanic.DomainInfo
{
    public class SagaInfo
    {
        public SagaInfo(Type sagaType)
        {
            Type = sagaType ?? throw new ArgumentNullException(nameof(sagaType));
            var sagaTypeInfo = sagaType.GetTypeInfo();
            if (!typeof(Saga).GetTypeInfo().IsAssignableFrom(sagaTypeInfo))
            {
                throw new ArgumentException("Attempt to build saga model for non-saga type!");
            }

            TriggerEventTypes = (from m in sagaType.GetRuntimeMethods()
                                 let p = m.GetParameters()
                                 where p.Length >= 1
                                 let t = p[0].ParameterType
                                 where typeof(Event).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())
                                 select t).ToArray();
        }

        public Type Type { get; }

        public Type[] TriggerEventTypes { get; }
    }
}