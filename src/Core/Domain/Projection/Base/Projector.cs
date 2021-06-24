namespace Cleanic.Core
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public abstract class Projector
    {
        public AggregateView CreateView(AggregateEvent @event)
        {
            var method = GetCreateMethod(@event.GetType());
            if (method == null) throw new Exception($"'{GetType().FullName}' don't know how to create aggregate view with '{@event.GetType().FullName}' event");
            return (AggregateView)method.Invoke(this, new Object[] { @event });
        }

        public void UpdateView(AggregateView view, AggregateEvent @event)
        {
            var method = GetUpdateMethod(@event.GetType());
            if (method == null) throw new Exception($"'{GetType().FullName}' don't know how to apply '{@event.GetType().FullName}' event");
            method.Invoke(this, new Object[] { view, @event });
        }

        public Expression<Func<AggregateView, Boolean>> GetFilter(AggregateEvent @event, AggregateViewInfo aggregateViewInfo)
        {
            var method = GetGetFilterMethod(@event.GetType());
            if (method == null) return aggregateViewInfo.BelongsToRootAggregate ? _ => true : view => view.AggregateId == @event.AggregateId;
            return (Expression<Func<AggregateView, Boolean>>)method.Invoke(this, new Object[] { @event });
        }

        private MethodInfo GetGetFilterMethod(Type eventType)
        {
            return GetType().GetTypeInfo().DeclaredMethods
                .Where(m => m.GetParameters().Length == 1)
                .Where(m => m.GetParameters()[0].ParameterType == eventType)
                .Where(m => m.ReturnType.GetGenericTypeDefinition() == typeof(Expression<>).GetGenericTypeDefinition())
                .SingleOrDefault();
        }

        private MethodInfo GetCreateMethod(Type eventType)
        {
            return GetType().GetTypeInfo().DeclaredMethods
                .Where(m => m.GetParameters().Length == 1)
                .Where(m => m.GetParameters()[0].ParameterType == eventType)
                .Where(m => m.ReturnType.IsSubclassOf(typeof(AggregateView)))
                .SingleOrDefault();
        }

        private MethodInfo GetUpdateMethod(Type eventType)
        {
            return GetType().GetTypeInfo().DeclaredMethods
                .Where(m => m.GetParameters().Length == 2)
                .Where(m => m.GetParameters()[0].ParameterType.IsSubclassOf(typeof(AggregateView)))
                .Where(m => m.GetParameters()[1].ParameterType == eventType)
                .SingleOrDefault();
        }
    }

    public abstract class Projector<T> : Projector where T : IAggregate { }
}