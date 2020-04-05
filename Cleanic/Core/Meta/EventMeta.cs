using System;
using System.Collections.Generic;

namespace Cleanic.Core
{
    public class EventMeta : IEquatable<EventMeta>
    {
        public EventMeta(Type eventType, EntityMeta entityMeta)
        {
            Type = eventType ?? throw new ArgumentNullException(nameof(eventType));
            Name = eventType.Name;
            Entity = entityMeta ?? throw new ArgumentNullException(nameof(entityMeta));
        }

        public String Name { get; }
        public Type Type { get; }
        public EntityMeta Entity { get; }

        public override Boolean Equals(Object obj) => Equals(obj as EventMeta);

        public Boolean Equals(EventMeta other)
        {
            return other != null && EqualityComparer<Type>.Default.Equals(Type, other.Type);
        }

        public override Int32 GetHashCode()
        {
            return 2049151605 + EqualityComparer<Type>.Default.GetHashCode(Type);
        }

        public override String ToString() => Name;
    }
}