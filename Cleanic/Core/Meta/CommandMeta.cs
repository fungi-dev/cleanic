using System;
using System.Collections.Generic;

namespace Cleanic.Core
{
    public class CommandMeta : IEquatable<CommandMeta>
    {
        public CommandMeta(Type commandType, EntityMeta entityMeta)
        {
            Type = commandType ?? throw new ArgumentNullException(nameof(commandType));
            Entity = entityMeta ?? throw new ArgumentNullException(nameof(entityMeta));
        }

        public Type Type { get; }
        public EntityMeta Entity { get; }

        public override Boolean Equals(Object obj) => Equals(obj as CommandMeta);

        public Boolean Equals(CommandMeta other)
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