namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class ProjectorInfo : DomainObjectInfo
    {
        public AggregateViewInfo AggregateView { get; }
        public Boolean IsRoot { get; }
        public IReadOnlyCollection<AggregateEventInfo> CreateEvents { get; internal set; }
        public IReadOnlyCollection<AggregateEventInfo> UpdateEvents { get; internal set; }

        public ProjectorInfo(Type projectorType, AggregateViewInfo aggregateViewInfo, Boolean isRoot) : base(projectorType)
        {
            if (!projectorType.GetTypeInfo().IsSubclassOf(typeof(Projector))) throw new ArgumentOutOfRangeException(nameof(projectorType));
            AggregateView = aggregateViewInfo ?? throw new ArgumentNullException(nameof(aggregateViewInfo));

            IsRoot = isRoot;
        }
    }
}