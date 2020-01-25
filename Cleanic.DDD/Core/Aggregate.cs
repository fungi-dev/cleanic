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

    /// <summary>
    /// The root of domain objects tree.
    /// Such tree representing complex object, unit of change in the domain.
    /// Every change in aggregate embodied by appropriate event.
    /// </summary>
    public abstract class Aggregate<T> : Entity<T>, IAggregate
        where T : Aggregate<T>
    {
        protected Aggregate(IIdentity<T> id) : base(id) { }

        /// <summary>
        /// The number of occured events.
        /// </summary>
        public UInt32 Version { get; private set; }

        /// <summary>
        /// Events which haven't been persisted yet.
        /// </summary>
        public IReadOnlyCollection<IEvent> ProducedEvents => _changes.ToImmutableHashSet();

        /// <summary>
        /// Lead the aggregate to state when all events have been applied.
        /// </summary>
        /// <param name="history">Events to be applied.</param>
        public void LoadFromHistory(ICollection<IEvent> history)
        {
            if (history.Any(_ => _.EntityId != Id))
            {
                throw new Exception("Attempt to apply foreign events to aggregate!");
            }

            foreach (var @event in history) Apply(@event as IEvent<T>, false);
        }

        /// <summary>
        /// Upgrade this aggregate according to new event.
        /// Prepare event to be persisted by infrastructure.
        /// And update aggregate's state if needed.
        /// </summary>
        /// <remarks>All events produced by aggregate should be passed into this method.</remarks>
        protected void Apply(IEvent<T> eventData)
        {
            var @event = eventData.HappenedWith(Id, DateTime.UtcNow);
            Apply(@event, true);
        }

        /// <summary>
        /// Get method which applies event to aggregate's state.
        /// </summary>
        protected virtual MethodInfo GetApplierOfConcreteEvent(Type eventType)
        {
            foreach (var method in GetType().GetRuntimeMethods().Where(x => x.Name == "On"))
            {
                var parameters = method.GetParameters();
                if (parameters.Length != 1) continue;
                if (parameters[0].ParameterType == eventType) return method;
            }
            return null;
        }

        private readonly List<IEvent<T>> _changes = new List<IEvent<T>>();

        private void Apply(IEvent<T> @event, Boolean isFresh)
        {
            var applier = GetApplierOfConcreteEvent(@event.GetType());
            applier?.Invoke(this, new Object[] { @event });
            Version++;

            if (isFresh) _changes.Add(@event);
        }
    }
}