namespace Cleanic.Core
{
    using System;
    using System.Reflection;

    public class AggregateEventInfo : DomainObjectInfo
    {
        public AggregateInfo Aggregate { get; }
        public String FullName { get; }

        public AggregateEventInfo(Type aggregateEventType, AggregateLogicInfo aggregateLogicInfo) : base(aggregateEventType)
        {
            if (aggregateLogicInfo == null) throw new ArgumentNullException(nameof(aggregateLogicInfo));
            if (aggregateEventType == null) throw new ArgumentNullException(nameof(aggregateEventType));
            if (!aggregateEventType.GetTypeInfo().IsSubclassOf(typeof(AggregateEvent))) throw new ArgumentOutOfRangeException(nameof(aggregateEventType));

            Aggregate = aggregateLogicInfo.Aggregate;
            FullName = aggregateEventType.FullName.Replace("+", ".");
        }
    }
}