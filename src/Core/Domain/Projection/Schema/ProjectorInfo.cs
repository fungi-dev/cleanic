namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;

    public class ProjectorInfo : DomainObjectInfo
    {
        public String FullName { get; }
        public AggregateViewInfo View { get; }
        public Boolean IsRoot { get; }
        public IReadOnlyCollection<AggregateEventInfo> Events { get; internal set; }

        public ProjectorInfo(Type projectorType, AggregateInfo aggregateInfo) : base(projectorType, aggregateInfo)
        {
            FullName = projectorType.FullName.Replace("+", ".");
            IsRoot = aggregateInfo.IsRoot;
        }
    }
}