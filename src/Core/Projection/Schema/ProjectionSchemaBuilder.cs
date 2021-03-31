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
        public ProjectionSchemaBuilder(DomainSchema domainSchema)
        {
            _domainSchema = domainSchema;
        }

        public ProjectionSchema BuildFromAssembly(Assembly assembly)
        {
            var projectorTypes = FindProjectorTypesInAssembly(assembly, _domainSchema.Language);
            _projectorInfos.AddRange(projectorTypes.Select(x => ProduceProjectorInfoFromType(x.Item1, x.Item2)));

            return Build();
        }

        public ProjectionSchemaBuilder Add<T>()
        {
            var type = typeof(T).GetTypeInfo();

            if (type.IsSubclassOf(typeof(Projector)))
            {
                var aggTypeArg = type.BaseType.GenericTypeArguments.Single();
                var aggregateInfo = _domainSchema.Language.Aggregates.SingleOrDefault(x => x.Type == aggTypeArg);
                if (aggregateInfo == null)
                {
                    var m = $"There is no aggregate in language for projector '{type.FullName}'";
                    throw new DomainSchemaException(m);
                }
                _projectorInfos.Add(ProduceProjectorInfoFromType(aggregateInfo, type));
            }
            else
            {
                throw new ArgumentOutOfRangeException("T");
            }

            return this;
        }

        public ProjectionSchema Build()
        {
            return new ProjectionSchema
            {
                Language = _domainSchema.Language,
                Projectors = _projectorInfos.ToImmutableHashSet()
            };
        }

        private readonly DomainSchema _domainSchema;
        private readonly List<ProjectorInfo> _projectorInfos = new List<ProjectorInfo>();

        private IEnumerable<(AggregateInfo, Type)> FindProjectorTypesInAssembly(Assembly projectionAssembly, LanguageSchema languageSchema)
        {
            var projectorTypes = projectionAssembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(Projector)));
            foreach (var agg in languageSchema.Aggregates)
            {
                var aggProjectorTypes = projectorTypes.Where(x => x.GenericTypeArguments.Contains(agg.Type));
                foreach (var aggProjectorType in aggProjectorTypes)
                {
                    yield return (agg, aggProjectorType);
                }
            }
        }

        private ProjectorInfo ProduceProjectorInfoFromType(AggregateInfo aggregateInfo, Type projectorType)
        {
            var projectorInfo = new ProjectorInfo(projectorType, aggregateInfo);

            var eventTypes = projectorInfo.Type.GetTypeInfo().DeclaredMethods
                .SelectMany(m => m.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(AggregateEvent)))
                .Distinct();
            projectorInfo.Events = eventTypes.Select(t => _domainSchema.GetAggregateEvent(t)).ToImmutableHashSet();

            return projectorInfo;
        }
    }
}