namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <remarks>
    /// Можно было не создавать отдельный класс для построения схемы, а при создании схемы, чтобы она сама сканила свою ассембли и находила агрегаты.
    /// Но это не позволяет запускать приложения, которые должны работать на основе ограниченной части предметки.
    /// А такая ситуация может возникнуть при определении отдельного приложения для построения особо нагруженной вьюхи, например.
    /// Билдер выделен в отдельный класс именно потому, что в подобных ситуациях процесс построения схемы состоит из нескольких шагов.
    /// </remarks>
    public class LanguageSchemaBuilder
    {
        public LanguageSchema BuildFromAssembly(Assembly assembly)
        {
            var types = FindEntityTypesInAssembly(assembly);
            _entityInfos = types.Select(x => ProduceEntityInfoFromType(x)).ToList();

            return Build();
        }

        public LanguageSchemaBuilder Add<T>()
        {
            _entityInfos.Add(ProduceEntityInfoFromType(typeof(T)));
            return this;
        }

        public LanguageSchema Build()
        {
            return new LanguageSchema { Entities = _entityInfos.ToImmutableHashSet() };
        }

        private List<EntityInfo> _entityInfos = new();

        private IEnumerable<Type> FindEntityTypesInAssembly(Assembly languageAssembly) => languageAssembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(Entity)));

        private EntityInfo ProduceEntityInfoFromType(Type entityType)
        {
            var entityInfo = new EntityInfo(entityType);

            var nested = entityType.GetTypeInfo().DeclaredNestedTypes;

            var commandTypes = nested.Where(x => x.IsSubclassOf(typeof(Command)));
            entityInfo.Commands = commandTypes.Select(x => new CommandInfo(x)).ToImmutableHashSet();

            var viewTypes = nested.Where(x => x.IsSubclassOf(typeof(View)));
            entityInfo.Views = viewTypes.Select(x => new ViewInfo(x)).ToImmutableHashSet();
            foreach (var viewInfo in entityInfo.Views)
            {
                var queryTypes = viewInfo.Type.GetTypeInfo().DeclaredNestedTypes.Where(x => typeof(Query).GetTypeInfo().IsAssignableFrom(x));
                viewInfo.Queries = queryTypes.Select(x => new QueryInfo(x)).ToImmutableHashSet();
            }

            return entityInfo;
        }
    }
}