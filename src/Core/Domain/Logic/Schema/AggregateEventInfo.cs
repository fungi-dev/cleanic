namespace Cleanic.Core
{
    using System;
    using System.Reflection;

    public class AggregateEventInfo : DomainObjectInfo
    {
        public AggregateEventInfo(Type aggregateEventType) : base(aggregateEventType)
        {
            if (aggregateEventType == null) throw new ArgumentNullException(nameof(aggregateEventType));
            if (!aggregateEventType.GetTypeInfo().IsSubclassOf(typeof(AggregateEvent))) throw new ArgumentOutOfRangeException(nameof(aggregateEventType));
        }
    }
}