using System;
using System.Collections.Generic;

namespace Cleanic.Application
{
    public class AggregateItemInfo : TermInfo, IEquatable<AggregateItemInfo>
    {
        public AggregateInfo Aggregate { get; }

        public AggregateItemInfo(Type aggregateItemTermType, AggregateInfo aggregate) : base(aggregateItemTermType)
        {
            Aggregate = aggregate ?? throw new ArgumentNullException(nameof(aggregate));
        }

        public override Boolean Equals(Object obj) => Equals(obj as AggregateItemInfo);

        public Boolean Equals(AggregateItemInfo other)
        {
            return other != null && EqualityComparer<Type>.Default.Equals(Type, other.Type);
        }

        public override Int32 GetHashCode()
        {
            return 2049151605 + EqualityComparer<Type>.Default.GetHashCode(Type);
        }

        public override String ToString() => Type.Name;
    }
}