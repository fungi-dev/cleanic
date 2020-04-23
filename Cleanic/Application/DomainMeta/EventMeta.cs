using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cleanic.Application
{
    public class EventMeta : DomainObjectMeta, IEquatable<EventMeta>
    {
        public AggregateMeta Aggregate { get; }

        public EventMeta(TypeInfo eventType, AggregateMeta aggregateMeta) : base(eventType)
        {
            Aggregate = aggregateMeta ?? throw new ArgumentNullException(nameof(aggregateMeta));
        }

        public override Boolean Equals(Object obj) => Equals(obj as EventMeta);

        public Boolean Equals(EventMeta other)
        {
            return other != null && EqualityComparer<Type>.Default.Equals(Type, other.Type);
        }

        public override Int32 GetHashCode()
        {
            return 2049151605 + EqualityComparer<Type>.Default.GetHashCode(Type);
        }
    }
}