namespace Cleanic.Core
{
    using System;
    using System.Reflection;

    public class DomainEventInfo : MessageInfo
    {
        public DomainEventInfo(Type domainEventType, AggregateInfo aggregate) : base(domainEventType, aggregate)
        {
            if (!domainEventType.GetTypeInfo().IsSubclassOf(typeof(DomainEvent))) throw new ArgumentOutOfRangeException(nameof(domainEventType));
        }
    }
}