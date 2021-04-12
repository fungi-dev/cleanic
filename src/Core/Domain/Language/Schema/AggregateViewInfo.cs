namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class AggregateViewInfo : DomainObjectInfo
    {
        public IReadOnlyCollection<QueryInfo> Queries { get; internal set; }

        public AggregateViewInfo(Type aggregateViewType) : base(aggregateViewType)
        {
            if (!aggregateViewType.GetTypeInfo().IsSubclassOf(typeof(AggregateView))) throw new ArgumentOutOfRangeException(nameof(aggregateViewType));
        }
    }
}