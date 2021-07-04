namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public class ProjectorInfo : DomainObjectInfo
    {
        public ViewInfo View { get; }
        public IReadOnlyCollection<EventInfo> CreateEvents { get; internal set; }
        public IReadOnlyCollection<EventInfo> UpdateEvents { get; internal set; }

        public ProjectorInfo(Type projectorType, ViewInfo viewInfo) : base(projectorType)
        {
            EnsureTermTypeCorrect(projectorType, typeof(Projector));
            View = viewInfo ?? throw new ArgumentNullException(nameof(viewInfo));

            CreateEvents = Array.Empty<EventInfo>().ToImmutableHashSet();
            UpdateEvents = Array.Empty<EventInfo>().ToImmutableHashSet();
        }
    }
}