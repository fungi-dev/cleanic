using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cleanic
{
    public class AggregateInfo : TermInfo
    {
        public IReadOnlyCollection<CommandInfo> Commands { get; internal set; }
        public IReadOnlyCollection<EventInfo> Events { get; internal set; }
        public IReadOnlyCollection<QueryInfo> Queries { get; internal set; }
        public bool IsRoot { get; }

        public AggregateInfo(Type aggregateType) : base(aggregateType)
        {
            IsRoot = aggregateType.GetTypeInfo().GetDeclaredField("RootId") != null;
        }
    }
}