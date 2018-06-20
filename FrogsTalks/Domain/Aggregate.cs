using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FrogsTalks.Domain
{
    /// <summary>
    /// The root of domain objects tree.
    /// Such tree representing complex object, unit of change in the domain.
    /// Every change in aggregate embodied by appropriate event.
    /// </summary>
    public abstract class Aggregate : Entity
    {
        protected Aggregate(Guid id) : base(id) { }

        /// <summary>
        /// The number of occured events.
        /// </summary>
        public Int32 Version { get; private set; }

        /// <summary>
        /// Events which haven't been persisted yet.
        /// </summary>
        public IEnumerable<Event> FreshChanges => _changes;

        /// <summary>
        /// Lead the aggregate to state when all events have been applied.
        /// </summary>
        /// <param name="history">Events to be applied.</param>
        public void LoadFromHistory(ICollection<Event> history)
        {
            if (history.Any(_ => _.AggregateId != Id))
            {
                throw new Exception("Attempt to apply foreign events to aggregate!");
            }

            foreach (var @event in history) Apply(@event, true);
        }

        /// <summary>
        /// Upgrade this aggregate according to new event.
        /// Prepare event to be persisted by infrastructure.
        /// And update aggregate's state if needed.
        /// </summary>
        /// <remarks>All events produced by aggregate should be passed into this method.</remarks>
        protected void Apply(Event @event)
        {
            @event.AggregateId = Id;
            Apply(@event, false);
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

        private readonly List<Event> _changes = new List<Event>();

        private void Apply(Event @event, Boolean isPersisted)
        {
            var applier = GetApplierOfConcreteEvent(@event.GetType());
            applier?.Invoke(this, new Object[] { @event });
            Version++;

            if (!isPersisted) _changes.Add(@event);
        }
    }
}