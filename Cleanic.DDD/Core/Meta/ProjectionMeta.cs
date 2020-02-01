using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Cleanic.Core
{
    public class ProjectionMeta : IProjectionMeta
    {
        public ProjectionMeta(Type projectionType)
        {
            Type = projectionType ?? throw new ArgumentNullException(nameof(projectionType));

            _applyMethods = Type.GetRuntimeMethods()
                .Where(x => x.GetParameters().Length == 1)
                .Where(x => x.GetParameters()[0].ParameterType.IsEvent())
                .ToArray();
            var events = _applyMethods.Select(x => x.GetParameters()[0].ParameterType);
            Events = events.Select(x => new EventMeta(x)).ToImmutableHashSet();
        }

        public Type Type { get; }
        public IReadOnlyCollection<EventMeta> Events { get; }

        public void RunApplier(IProjection projection, IEvent @event)
        {
            var applier = _applyMethods.Single(x => x.GetParameters()[0].ParameterType == @event.GetType());
            applier.Invoke(projection, new Object[] { @event });
        }

        public IIdentity GetProjectionIdFromAffectingEvent(IEvent @event)
        {
            var staticMethods = Type.GetRuntimeMethods().Where(x => x.IsStatic);
            var getIdMethod = staticMethods.Where(x => x.GetParameters().Length == 1).SingleOrDefault(x => x.GetParameters()[0].ParameterType.IsEvent());
            return (IIdentity)getIdMethod?.Invoke(null, new[] { @event }) ?? @event.EntityId;
        }

        private readonly MethodInfo[] _applyMethods;
    }
}