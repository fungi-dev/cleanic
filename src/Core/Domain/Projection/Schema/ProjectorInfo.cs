namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    public sealed class ProjectorInfo : DomainObjectInfo
    {
        public static ProjectorInfo Get(Type type) => (ProjectorInfo)Get(type, () => new ProjectorInfo(type));

        private ProjectorInfo(Type projectorType) : base(projectorType)
        {
            EnsureTermTypeCorrect<Projector>(projectorType);
            
            var viewType = projectorType
                .GetRuntimeMethods()
                .SelectMany(m => m.GetParameters())
                .Select(p => p.ParameterType)
                .Where(p => p.IsSubclassOf(typeof(View)))
                .Distinct()
                .Single();
            View = ViewInfo.Get(viewType);

            var createEventTypes = Type.GetTypeInfo().DeclaredMethods
                .Where(m => m.ReturnType.IsSubclassOf(typeof(View)))
                .Where(m => m.GetParameters().Length == 1)
                .SelectMany(m => m.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => t.IsSubclassOf(typeof(Event)))
                .Distinct();
            CreateEvents = createEventTypes.Select(t => EventInfo.Get(t)).ToImmutableHashSet();

            var updateEventTypes = Type.GetTypeInfo().DeclaredMethods
                .Where(m => m.ReturnType == typeof(void))
                .Where(m => m.GetParameters().Length == 2)
                .SelectMany(m => m.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => t.IsSubclassOf(typeof(Event)))
                .Distinct();
            UpdateEvents = updateEventTypes.Select(t => EventInfo.Get(t)).ToImmutableHashSet();
        }
   
        public ViewInfo View { get; }
        public IReadOnlyCollection<EventInfo> CreateEvents { get; }
        public IReadOnlyCollection<EventInfo> UpdateEvents { get; }
 }
}