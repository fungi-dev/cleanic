namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    public sealed class EntityInfo : DomainObjectInfo
    {
        public static EntityInfo Get(Type type) => (EntityInfo)Get(type, () => new EntityInfo(type));

        private EntityInfo(Type entityType) : base(entityType)
        {
            EnsureTermTypeCorrect<Entity>(entityType);

            var nested = Type.GetTypeInfo().DeclaredNestedTypes;

            var commandTypes = nested.Where(x => x.IsSubclassOf(typeof(Command)));
            Commands = commandTypes.Select(x => CommandInfo.Get(x)).ToImmutableHashSet();
            EnsureExactOneInitialCommand(Commands);

            var viewTypes = nested.Where(x => x.IsSubclassOf(typeof(View)));
            Views = viewTypes.Select(x => ViewInfo.Get(x)).ToImmutableHashSet();
        }

        public IReadOnlyCollection<CommandInfo> Commands { get; }
        public IReadOnlyCollection<ViewInfo> Views { get; }

        private void EnsureExactOneInitialCommand(IEnumerable<CommandInfo> commandInfos)
        {
            var initialCommands = commandInfos.Where(i => i.IsInitial).ToArray();

            if (initialCommands.Length == 0) throw new LanguageSchemaException($"Entity '{Type.FullName}' has no initial commands");
            if (initialCommands.Length > 1) throw new LanguageSchemaException($"Entity '{Type.FullName}' has more than one initial commands");
        }
    }
}