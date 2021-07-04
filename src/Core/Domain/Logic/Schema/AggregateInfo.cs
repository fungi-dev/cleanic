namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Collections.ObjectModel;

    public class AggregateInfo : DomainObjectInfo
    {
        public EntityInfo Entity { get; }
        public IReadOnlyCollection<EventInfo> Events { get; internal set; }
        public IReadOnlyDictionary<CommandInfo, IReadOnlyCollection<ServiceInfo>> Dependencies { get; internal set; }

        public AggregateInfo(Type aggregateType, EntityInfo entityInfo) : base(aggregateType)
        {
            EnsureTermTypeCorrect(aggregateType, typeof(Aggregate));
            Entity = entityInfo ?? throw new ArgumentNullException(nameof(entityInfo));
            if (Id != Entity.Id) throw new LogicSchemaException("Aggregate must has same identifier as linked Entity");
            Name = Entity.Name;

            Events = Array.Empty<EventInfo>().ToImmutableHashSet();
            Dependencies = new ReadOnlyDictionary<CommandInfo, IReadOnlyCollection<ServiceInfo>>(new Dictionary<CommandInfo, IReadOnlyCollection<ServiceInfo>>());
        }
    }
}