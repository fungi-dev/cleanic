namespace Cleanic.Core
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public abstract class Projector
    {
        public View CreateView(Event @event)
        {
            var method = GetCreateMethod(@event.GetType());
            if (method == null) throw new Exception($"'{GetType().FullName}' don't know how to create view with '{@event.GetType().FullName}' event");
            return (View)method.Invoke(this, new Object[] { @event });
        }

        public void UpdateView(View view, Event @event)
        {
            var method = GetUpdateMethod(@event.GetType());
            if (method == null) throw new Exception($"'{GetType().FullName}' don't know how to apply '{@event.GetType().FullName}' event");
            method.Invoke(this, new Object[] { view, @event });
        }

        public Expression<Func<View, Boolean>> GetFilter(Event @event)
        {
            var method = GetGetFilterMethod(@event.GetType());
            if (method == null) return view => view.EntityId == @event.EntityId;
            return (Expression<Func<View, Boolean>>)method.Invoke(this, new Object[] { @event });
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
                .Where(m => m.ReturnType.IsSubclassOf(typeof(View)))
                .SingleOrDefault();
        }

        private MethodInfo GetUpdateMethod(Type eventType)
        {
            return GetType().GetTypeInfo().DeclaredMethods
                .Where(m => m.GetParameters().Length == 2)
                .Where(m => m.GetParameters()[0].ParameterType.IsSubclassOf(typeof(View)))
                .Where(m => m.GetParameters()[1].ParameterType == eventType)
                .SingleOrDefault();
        }
    }

    public abstract class Projector<T> : Projector where T : Entity { }
}