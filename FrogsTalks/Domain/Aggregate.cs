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
    public class Aggregate : Entity
    {
        /// <summary>
        /// The number of events with this aggregate.
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// Move the aggregate to state when all supplied events already occured.
        /// </summary>
        public void ApplyEvents(IEnumerable<IEvent> events)
        {
            foreach (var e in events)
            {
                MethodInfo applier = null;
                foreach (var m in GetType().GetRuntimeMethods().Where(x => x.Name == "Apply"))
                {
                    if (m.Name != "Apply") continue;
                    var parameters = m.GetParameters();
                    if (parameters.Length != 1) continue;
                    if (parameters[0].ParameterType is IEvent) applier = m;
                }
                if (applier == null)
                {
                    var m = $"Aggregate {GetType().Name} does not know how to apply event {e.GetType().Name}";
                    throw new InvalidOperationException(m);
                }

                applier.Invoke(this, new[] { e });
                Version++;
            }
        }
    }
}