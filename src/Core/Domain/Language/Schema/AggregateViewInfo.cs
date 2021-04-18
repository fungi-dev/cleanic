namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class AggregateViewInfo : DomainObjectInfo
    {
        public IReadOnlyCollection<QueryInfo> Queries { get; internal set; }
        public Boolean IsRoot { get; }

        public AggregateViewInfo(Type aggregateViewType, Boolean isRoot) : base(aggregateViewType)
        {
            if (!aggregateViewType.GetTypeInfo().IsSubclassOf(typeof(AggregateView))) throw new ArgumentOutOfRangeException(nameof(aggregateViewType));

            IsRoot = isRoot;
        }
    }
}