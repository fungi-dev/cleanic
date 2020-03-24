using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Cleanic.Core
{
    public class ProjectionMeta : IProjectionMeta
    {
        public ProjectionMeta(Type projectionType, DomainFacade domain)
        {
            Type = projectionType ?? throw new ArgumentNullException(nameof(projectionType));
            Name = projectionType.Name;
            _domain = domain ?? throw new ArgumentNullException(nameof(domain));

            var nestedTypes = Type.GetTypeInfo().DeclaredNestedTypes;
            Queries = nestedTypes.Where(x => x.IsQuery()).Select(x => x.AsType()).ToImmutableHashSet();

            _applyMethods = Type.GetRuntimeMethods()
                .Where(x => !x.IsStatic)
                .Where(x => x.GetParameters().Length == 1)
                .Where(x => x.GetParameters()[0].ParameterType.IsEvent())
                .ToArray();
        }

        public String Name { get; }
        public Type Type { get; }
        public IReadOnlyCollection<Type> Queries { get; }
        public IReadOnlyCollection<EventMeta> Events
        {
            get
            {
                if (_events != null) return _events;
                var eventTypes = _applyMethods.Select(x => x.GetParameters()[0].ParameterType);
                return _events = eventTypes.Select(x => _domain.GetEventMeta(x)).ToImmutableHashSet();
            }
        }

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
        private readonly DomainFacade _domain;
        private ImmutableHashSet<EventMeta> _events;
    }
}