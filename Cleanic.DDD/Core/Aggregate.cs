using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Cleanic.Core
{
    public interface IAggregate : IEntity
    {
        UInt32 Version { get; }
        IReadOnlyCollection<IEvent> ProducedEvents { get; }

        void LoadFromHistory(ICollection<IEvent> history);
    }

    public abstract class Aggregate<T> : IAggregate
        where T : Entity<T>
    {
        public Aggregate(IIdentity id)
        {
            State = (T)Activator.CreateInstance(typeof(T), id);
        }

        public IIdentity Id => State?.Id;
        public T State { get; }
        public UInt32 Version { get; private set; }

        public IReadOnlyCollection<IEvent> ProducedEvents => _changes.ToImmutableHashSet();

        public void LoadFromHistory(ICollection<IEvent> history)
        {
            foreach (var e in history)
            {
                var stateOfEventType = e.GetType().GetTypeInfo().BaseType.GenericTypeArguments.Single();
                if (stateOfEventType != typeof(T)) throw new Exception("Attempt to apply foreign events to aggregate!");
            }

            foreach (var @event in history) Apply(@event, false);
        }

        protected Boolean TryValidate(ICommand command)
        {
            var methods = GetType().GetRuntimeMethods()
                .Where(x => x.GetParameters().Length == 1)
                .Where(x => x.ReturnType.IsErrorCollection());
            var checker = methods.SingleOrDefault(x => x.GetParameters()[0].ParameterType == command.GetType());

            var errors = new List<IError>();
            if (checker != null) errors.AddRange((IEnumerable<IError>)checker.Invoke(this, new[] { command }));
            foreach (var error in errors) Apply(error, true);

            return !errors.Any();
        }

        protected void Apply(IEvent<T> @event)
        {
            Apply(@event, true);
        }

        private MethodInfo GetApplierOfConcreteEvent(Type eventType)
        {
            var methods = typeof(T).GetRuntimeMethods().Where(x => x.GetParameters().Length == 1);
            return methods.Single(x => x.GetParameters()[0].ParameterType == eventType);
        }

        private readonly List<IEvent> _changes = new List<IEvent>();

        private void Apply(IEvent @event, Boolean isFresh)
        {
            var applier = GetApplierOfConcreteEvent(@event.GetType());
            applier?.Invoke(State, new Object[] { @event });
            Version++;

            if (isFresh) _changes.Add(@event);
        }
    }
}