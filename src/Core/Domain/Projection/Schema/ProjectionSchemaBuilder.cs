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
                var aggTypeArg = type.BaseType.GenericTypeArguments.Single();
                var aggregateInfo = _logicSchema.Language.GetAggregate(aggTypeArg);
                var aggregateViewInfo = FindAggregateViewForProjector(type);
                _projectorInfos.Add(ProduceProjectorInfoFromType(aggregateInfo, aggregateViewInfo, type));
            }
            else
            {
                var m = $"Type '{type.FullName}' added in projection schema as a projector but it isn't subclass of Projector";
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
        private readonly List<ProjectorInfo> _projectorInfos = new List<ProjectorInfo>();

        private IEnumerable<(AggregateInfo, AggregateViewInfo, Type)> FindProjectorTypesInAssembly(Assembly projectionAssembly, LanguageSchema languageSchema)
        {
            var projectorTypes = projectionAssembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(Projector)));
            foreach (var agg in languageSchema.Aggregates)
            {
                var aggProjectorTypes = projectorTypes.Where(x => x.BaseType.GenericTypeArguments.Contains(agg.Type));
                foreach (var aggProjectorType in aggProjectorTypes)
                {
                    yield return (agg, FindAggregateViewForProjector(aggProjectorType), aggProjectorType);
                }
            }
        }

        private AggregateViewInfo FindAggregateViewForProjector(Type projectorType)
        {
            var aggregateViewTypes = projectorType
                .GetRuntimeMethods()
                .SelectMany(m => m.GetParameters())
                .Select(p => p.ParameterType)
                .Where(p => p.IsSubclassOf(typeof(AggregateView)))
                .Distinct()
                .ToArray();
            if (aggregateViewTypes.Length == 0) throw new ProjectionSchemaException("Projector has no aggregate view update methods");
            if (aggregateViewTypes.Length > 1) throw new ProjectionSchemaException("Projector has update methods for many aggregate views");
            return _logicSchema.Language.GetAggregateView(aggregateViewTypes.Single());
        }

        private ProjectorInfo ProduceProjectorInfoFromType(AggregateInfo aggregateInfo, AggregateViewInfo aggregateViewInfo, Type projectorType)
        {
            var projectorInfo = new ProjectorInfo(projectorType, aggregateViewInfo, aggregateInfo.IsRoot);

            var createEventTypes = projectorInfo.Type.GetTypeInfo().DeclaredMethods
                .Where(m => m.ReturnType.IsSubclassOf(typeof(AggregateView)))
                .Where(m => m.GetParameters().Length == 1)
                .SelectMany(m => m.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => t.IsSubclassOf(typeof(AggregateEvent)))
                .Distinct();
            projectorInfo.CreateEvents = createEventTypes.Select(t => _logicSchema.GetAggregateEvent(t)).ToImmutableHashSet();

            var updateEventTypes = projectorInfo.Type.GetTypeInfo().DeclaredMethods
                .Where(m => m.ReturnType == typeof(void))
                .Where(m => m.GetParameters().Length == 2)
                .SelectMany(m => m.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => t.IsSubclassOf(typeof(AggregateEvent)))
                .Distinct();
            projectorInfo.UpdateEvents = updateEventTypes.Select(t => _logicSchema.GetAggregateEvent(t)).ToImmutableHashSet();

            return projectorInfo;
        }
    }
}