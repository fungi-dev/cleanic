using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cleanic.Application
{
    public class CommandMeta : DomainObjectMeta, IEquatable<CommandMeta>
    {
        public AggregateMeta Aggregate { get; }

        public CommandMeta(TypeInfo commandType, AggregateMeta aggregateMeta) : base(commandType)
        {
            Aggregate = aggregateMeta ?? throw new ArgumentNullException(nameof(aggregateMeta));
        }

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