namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class AggregateInfo : DomainObjectInfo
    {
        public IReadOnlyCollection<CommandInfo> Commands { get; internal set; }
        public IReadOnlyCollection<AggregateViewInfo> Views { get; internal set; }
        public Boolean IsRoot { get; }

        public AggregateInfo(Type aggregateType) : base(aggregateType)
        {
            if (!aggregateType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IAggregate)))
            {
                var m = $"Type '{aggregateType.FullName}' added in language schema as an aggregate but it doesn't implement IAggregate interface";
                throw new LanguageSchemaException(m);
            }

            IsRoot = aggregateType.GetTypeInfo().GetDeclaredField("RootId") != null;
        }
    }
}