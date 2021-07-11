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
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var entityTypes = assembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(Entity)));
            foreach (var entityType in entityTypes) Add(entityType);

            return Build();
        }

        public LanguageSchemaBuilder Add<T>() where T : Entity
        {
            Add(typeof(T));
            return this;
        }

        public LanguageSchemaBuilder Add(Type entityType)
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));

            _entityInfos.Add(EntityInfo.Get(entityType));
            return this;
        }

        public LanguageSchema Build()
        {
            return new LanguageSchema { Entities = _entityInfos.ToImmutableHashSet() };
        }

        private readonly List<EntityInfo> _entityInfos = new();
    }
}