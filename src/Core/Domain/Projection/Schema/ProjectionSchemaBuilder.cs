namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <remarks>
    /// Можно было не создавать отдельный класс для построения схемы, а при создании схемы, чтобы она сама сканила свою ассембли и находила агрегаты/саги/сервисы.
    /// Но это не позволяет запускать приложения, которые должны работать на основе ограниченной части предметки.
    /// А такая ситуация может возникнуть при определении отдельного приложения для построения особо нагруженной вьюхи, например.
    /// Билдер выделен в отдельный класс именно потому, что в подобных ситуациях процесс построения схемы состоит из нескольких шагов.
    /// </remarks>
    public class ProjectionSchemaBuilder
    {
        public ProjectionSchemaBuilder(LogicSchema logicSchema)
        {
            _logicSchema = logicSchema;
        }

        public ProjectionSchema BuildFromAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var projectorTypes = assembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(Projector)));
            foreach (var projectorType in projectorTypes) Add(projectorType);

            return Build();
        }

        public ProjectionSchemaBuilder Add<T>() where T : Projector
        {
            Add(typeof(T));
            return this;
        }

        public ProjectionSchemaBuilder Add(Type projectorType)
        {
            if (projectorType == null) throw new ArgumentNullException(nameof(projectorType));

            _projectorInfos.Add(ProjectorInfo.Get(projectorType));
            return this;
        }

        public ProjectionSchema Build()
        {
            return new ProjectionSchema
            {
                Language = _logicSchema.Language,
                Projectors = _projectorInfos.ToImmutableHashSet()
            };
        }

        private readonly LogicSchema _logicSchema;
        private readonly List<ProjectorInfo> _projectorInfos = new();
    }
}