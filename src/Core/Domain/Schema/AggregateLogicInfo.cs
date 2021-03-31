namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    public class AggregateLogicInfo : DomainObjectInfo
    {
        public AggregateInfo Aggregate { get; }
        public IReadOnlyCollection<AggregateEventInfo> Events { get; internal set; }
        public IReadOnlyDictionary<CommandInfo, IReadOnlyCollection<ServiceInfo>> Dependencies { get; internal set; }

        public AggregateLogicInfo(Type aggregateLogicType, AggregateInfo aggregateInfo) : base(aggregateLogicType)
        {
            if (!aggregateLogicType.GetTypeInfo().IsSubclassOf(typeof(AggregateLogic))) throw new ArgumentOutOfRangeException(nameof(aggregateLogicType));

            Aggregate = aggregateInfo ?? throw new ArgumentNullException(nameof(aggregateInfo));
            Name = Aggregate.Name;
            Dependencies = new ReadOnlyDictionary<CommandInfo, IReadOnlyCollection<ServiceInfo>>(new Dictionary<CommandInfo, IReadOnlyCollection<ServiceInfo>>());
        }
    }
}