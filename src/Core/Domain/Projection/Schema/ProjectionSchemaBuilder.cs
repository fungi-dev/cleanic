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
            var projectorTypes = FindProjectorTypesInAssembly(assembly, _logicSchema.Language);
            _projectorInfos.AddRange(projectorTypes.Select(x => ProduceProjectorInfoFromType(x.Item1, x.Item2, x.Item3)));

            return Build();
        }

        public ProjectionSchemaBuilder Add<T>()
        {
            var type = typeof(T).GetTypeInfo();

            if (type.IsSubclassOf(typeof(Projector)))
            {
                var entityTypeArg = type.BaseType.GenericTypeArguments.Single();
                var entityInfo = _logicSchema.Language.GetEntity(entityTypeArg);
                var viewInfo = FindViewForProjector(type);
                _projectorInfos.Add(ProduceProjectorInfoFromType(entityInfo, viewInfo, type));
            }
            else
            {
                var m = $"Type '{type.FullName}' added in projection schema as a projector but it isn't subclass of {nameof(Projector)}";
                throw new ProjectionSchemaException(m);
            }

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

        private IEnumerable<(EntityInfo, ViewInfo, Type)> FindProjectorTypesInAssembly(Assembly projectionAssembly, LanguageSchema languageSchema)
        {
            var projectorTypes = projectionAssembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(Projector)));
            foreach (var entity in languageSchema.Entities)
            {
                var entityProjectorTypes = projectorTypes.Where(x => x.BaseType.GenericTypeArguments.Contains(entity.Type));
                foreach (var entityProjectorType in entityProjectorTypes)
                {
                    yield return (entity, FindViewForProjector(entityProjectorType), entityProjectorType);
                }
            }
        }

        private ViewInfo FindViewForProjector(Type projectorType)
        {
            var viewTypes = projectorType
                .GetRuntimeMethods()
                .SelectMany(m => m.GetParameters())
                .Select(p => p.ParameterType)
                .Where(p => p.IsSubclassOf(typeof(View)))
                .Distinct()
                .ToArray();
            if (viewTypes.Length == 0) throw new ProjectionSchemaException("Projector has no view update methods");
            if (viewTypes.Length > 1) throw new ProjectionSchemaException("Projector has update methods for many views");
            return _logicSchema.Language.GetView(viewTypes.Single());
        }

        private ProjectorInfo ProduceProjectorInfoFromType(EntityInfo entityInfo, ViewInfo viewInfo, Type projectorType)
        {
            var projectorInfo = new ProjectorInfo(projectorType, viewInfo);

            var createEventTypes = projectorInfo.Type.GetTypeInfo().DeclaredMethods
                .Where(m => m.ReturnType.IsSubclassOf(typeof(View)))
                .Where(m => m.GetParameters().Length == 1)
                .SelectMany(m => m.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => t.IsSubclassOf(typeof(Event)))
                .Distinct();
            projectorInfo.CreateEvents = createEventTypes.Select(t => _logicSchema.GetEvent(t)).ToImmutableHashSet();

            var updateEventTypes = projectorInfo.Type.GetTypeInfo().DeclaredMethods
                .Where(m => m.ReturnType == typeof(void))
                .Where(m => m.GetParameters().Length == 2)
                .SelectMany(m => m.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => t.IsSubclassOf(typeof(Event)))
                .Distinct();
            projectorInfo.UpdateEvents = updateEventTypes.Select(t => _logicSchema.GetEvent(t)).ToImmutableHashSet();

            return projectorInfo;
        }
    }
}