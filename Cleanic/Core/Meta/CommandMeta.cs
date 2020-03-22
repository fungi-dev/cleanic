using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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

        public static Boolean operator ==(CommandMeta left, CommandMeta right)
        {
            return EqualityComparer<CommandMeta>.Default.Equals(left, right);
        }

        public static Boolean operator !=(CommandMeta left, CommandMeta right)
        {
            return !(left == right);
        }
    }

    public static class CommandTypeExtensions
    {
        public static Boolean IsCommand(this Type type) => type.GetTypeInfo().IsCommand();
        public static Boolean IsCommand(this TypeInfo type) => type.AsType() == typeof(ICommand) || type.ImplementedInterfaces.Contains(typeof(ICommand));
        public static Boolean IsCommandCollection(this Type type) => type.GetTypeInfo().IsCommandCollection();
        public static Boolean IsCommandCollection(this TypeInfo type)
        {
            if (type.IsSubclassOf(typeof(Task))) type = type.GenericTypeArguments[0].GetTypeInfo();
            if (!type.IsArray) return false;
            return type.GetElementType().IsCommand();
        }
    }
}