namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;

    public class ProjectorInfo
    {
        public Type Type { get; }
        public String Name { get; }
        public String FullName { get; }
        public AggregateInfo Aggregate { get; }
        public AggregateViewInfo View { get; }
        public Boolean IsRoot { get; }
        public IReadOnlyCollection<AggregateEventInfo> Events { get; internal set; }

        public ProjectorInfo(Type projectorType, AggregateInfo aggregateInfo)
        {
            Type = projectorType ?? throw new ArgumentNullException(nameof(projectorType));
            Name = projectorType.Name;
            FullName = projectorType.FullName.Replace("+", ".");
            Aggregate = aggregateInfo ?? throw new ArgumentNullException(nameof(aggregateInfo));
            IsRoot = aggregateInfo.IsRoot;
        }

        public override String ToString() => Name;
    }
}