using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cleanic
{
    public class ProjectionInfo
    {
        public Type Type { get; }
        public String Name { get; }
        public String FullName { get; }
        public AggregateInfo Aggregate { get; }
        public Boolean IsRoot { get; }
        public Boolean Materialized { get; internal set; }
        public IReadOnlyCollection<EventInfo> Events { get; internal set; }

        public ProjectionInfo(Type projectionType, AggregateInfo aggregateInfo)
        {
            Type = projectionType ?? throw new ArgumentNullException(nameof(projectionType));
            Name = projectionType.Name;
            FullName = projectionType.FullName.Replace("+", ".");
            Aggregate = aggregateInfo ?? throw new ArgumentNullException(nameof(aggregateInfo));
            IsRoot = aggregateInfo.IsRoot;
        }

        public override String ToString() => Name;

        public String GetIdFromEvent(Event @event)
        {
            var method = Type.GetTypeInfo().DeclaredMethods
                .Where(x => x.IsStatic)
                .Where(x => x.GetParameters().Length == 1)
                .SingleOrDefault(x => x.GetParameters()[0].ParameterType.GetTypeInfo().IsAssignableFrom(@event.GetType()));
            return (String)method?.Invoke(null, new[] { @event }) ?? @event.AggregateId;
        }
    }
}