using System;
using System.Linq;
using System.Reflection;

namespace Cleanic.Core
{
    public class Projection
    {
        public String AggregateId { get; set; }

        public void Apply(Event @event)
        {
            var type = @event.GetType();
            var method = GetApplyMethod(type);
            if (method == null) throw new Exception($"'{GetType().FullName}' don't know how to apply a '{type.FullName}'");
            method.Invoke(this, new[] { @event });
        }

        private MethodInfo GetApplyMethod(Type eventType)
        {
            return GetType().GetTypeInfo().DeclaredMethods
                .Where(m => !m.IsStatic)
                .Where(m => m.GetParameters().Length == 1)
                .Where(m => m.GetParameters()[0].ParameterType == eventType)
                .SingleOrDefault();
        }
    }
}