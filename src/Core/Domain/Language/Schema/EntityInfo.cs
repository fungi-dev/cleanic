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

            var viewTypes = nested.Where(x => x.IsSubclassOf(typeof(View)));
            Views = viewTypes.Select(x => ViewInfo.Get(x)).ToImmutableHashSet();
        }

        public IReadOnlyCollection<CommandInfo> Commands { get; }
        public IReadOnlyCollection<ViewInfo> Views { get; }
    }
}