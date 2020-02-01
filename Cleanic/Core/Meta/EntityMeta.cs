using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Cleanic.Core
{
    public class EntityMeta
    {
        public EntityMeta(Type entityType)
        {
            Type = entityType ?? throw new ArgumentNullException(nameof(entityType));
            Name = entityType.Name;

            var nestedTypes = Type.GetTypeInfo().DeclaredNestedTypes;

            var commandTypes = nestedTypes.Where(x => x.IsCommand());
            Commands = commandTypes.Select(x => new CommandMeta(x.AsType(), this)).ToImmutableHashSet();

            var eventTypes = nestedTypes.Where(x => x.IsEvent());
            Events = eventTypes.Select(x => new EventMeta(x.AsType())).ToImmutableHashSet();
        }

        public String Name { get; }
        public Type Type { get; }
        public IReadOnlyCollection<CommandMeta> Commands { get; }
        public IReadOnlyCollection<EventMeta> Events { get; }

        public override String ToString() => Name;
    }
}