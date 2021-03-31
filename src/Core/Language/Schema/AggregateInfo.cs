namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class AggregateInfo : TermInfo
    {
        public IReadOnlyCollection<CommandInfo> Commands { get; internal set; }
        public IReadOnlyCollection<DomainEventInfo> DomainEvents { get; internal set; }
        public IReadOnlyCollection<AggregateViewInfo> Views { get; internal set; }
        public Boolean IsRoot { get; }

        public AggregateInfo(Type aggregateType) : base(aggregateType)
        {
            if (!aggregateType.GetTypeInfo().IsSubclassOf(typeof(Aggregate))) throw new ArgumentOutOfRangeException(nameof(aggregateType));

            IsRoot = aggregateType.GetTypeInfo().GetDeclaredField("RootId") != null;
        }
    }
}