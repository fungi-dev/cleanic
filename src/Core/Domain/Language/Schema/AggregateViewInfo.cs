namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class AggregateViewInfo : MessageInfo
    {
        public IReadOnlyCollection<QueryInfo> Queries { get; internal set; }

        public AggregateViewInfo(Type aggregateViewType, Boolean belongsToRootAggregate) : base(aggregateViewType, belongsToRootAggregate)
        {
            if (!aggregateViewType.GetTypeInfo().IsSubclassOf(typeof(AggregateView))) throw new ArgumentOutOfRangeException(nameof(aggregateViewType));
        }
    }
}