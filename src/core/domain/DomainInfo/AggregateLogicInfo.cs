using System;
using System.Collections.Generic;

namespace Cleanic
{
    public class AggregateLogicInfo : DomainObjectInfo
    {
        public AggregateInfo AggregateInfo { get; }
        public IReadOnlyDictionary<CommandInfo, IReadOnlyCollection<ServiceInfo>> Dependencies { get; internal set; }

        public AggregateLogicInfo(Type aggregateLogicType, AggregateInfo aggregateInfo) : base(aggregateLogicType)
        {
            AggregateInfo = aggregateInfo ?? throw new ArgumentNullException(nameof(aggregateInfo));
        }
    }
}