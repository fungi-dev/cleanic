namespace Cleanic.Core
{
    using System;
    using System.Linq;
    using System.Reflection;

    public abstract class Projector
    {
        public void Apply(AggregateView view, AggregateEvent @event)
        {
            var method = GetApplyMethod(@event.GetType());
            if (method == null) throw new Exception($"'{GetType().FullName}' don't know how to apply a '{@event.GetType().FullName}'");
            method.Invoke(this, new Object[] { view, @event });
        }

        public String GetId(AggregateEvent @event)
        {
            var method = GetType().GetTypeInfo().DeclaredMethods
                .Where(x => !x.IsStatic)
                .Where(x => x.GetParameters().Length == 1)
                .SingleOrDefault(x => x.GetParameters()[0].ParameterType.GetTypeInfo().IsAssignableFrom(@event.GetType()));
            return (String)method?.Invoke(null, new[] { @event }) ?? @event.AggregateId;
        }

        private MethodInfo GetApplyMethod(Type eventType)
        {
            return GetType().GetTypeInfo().DeclaredMethods
                .Where(m => !m.IsStatic)
                .Where(m => m.GetParameters().Length == 2)
                .Where(m => m.GetParameters()[1].ParameterType == eventType)
                .SingleOrDefault();
        }
    }

    public abstract class Projector<T> : Projector where T : Aggregate { }
}