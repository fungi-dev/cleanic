namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    public class AggregateLogicInfo : DomainObjectInfo
    {
        public IReadOnlyCollection<AggregateEventInfo> Events { get; internal set; }
        public IReadOnlyDictionary<CommandInfo, IReadOnlyCollection<ServiceInfo>> Dependencies { get; internal set; }

        public AggregateLogicInfo(Type aggregateLogicType, AggregateInfo aggregateInfo) : base(aggregateLogicType, aggregateInfo)
        {
            if (!aggregateLogicType.GetTypeInfo().IsSubclassOf(typeof(Aggregate))) throw new ArgumentOutOfRangeException(nameof(aggregateLogicType));

            Dependencies = new ReadOnlyDictionary<CommandInfo, IReadOnlyCollection<ServiceInfo>>(new Dictionary<CommandInfo, IReadOnlyCollection<ServiceInfo>>());
        }
    }
}