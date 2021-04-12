namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    public class AggregateLogicInfo : DomainObjectInfo
    {
        public AggregateInfo AggregateFromLanguage { get; }
        public IReadOnlyCollection<AggregateEventInfo> Events { get; internal set; }
        public IReadOnlyDictionary<CommandInfo, IReadOnlyCollection<ServiceInfo>> Dependencies { get; internal set; }

        public AggregateLogicInfo(Type aggregateLogicType, AggregateInfo aggregateFromLanguage) : base(aggregateLogicType)
        {
            if (!aggregateLogicType.GetTypeInfo().IsSubclassOf(typeof(Aggregate))) throw new ArgumentOutOfRangeException(nameof(aggregateLogicType));
            AggregateFromLanguage = aggregateFromLanguage ?? throw new ArgumentNullException(nameof(aggregateFromLanguage));
            if (Id != AggregateFromLanguage.Id) throw new LogicSchemaException("Aggregate identifiers from domain language and logic differs");
            Name = AggregateFromLanguage.Name;

            Dependencies = new ReadOnlyDictionary<CommandInfo, IReadOnlyCollection<ServiceInfo>>(new Dictionary<CommandInfo, IReadOnlyCollection<ServiceInfo>>());
        }
    }
}