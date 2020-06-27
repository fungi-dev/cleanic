using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Cleanic.Application
{
    public class ProjectionsInfoBuilder
    {
        public ProjectionsInfoBuilder(LanguageInfo languageInfo, Configuration configuration = null)
        {
            _languageInfo = languageInfo ?? throw new ArgumentNullException(nameof(languageInfo));
            _configuration = configuration ?? new Configuration();
        }

        public ProjectionsInfoBuilder Projection<TAggregate, TProjection>()
        {
            var aggregateInfo = _languageInfo.GetAggregate(typeof(TAggregate));
            if (!_projectionTypes.TryGetValue(aggregateInfo, out var types))
            {
                _projectionTypes.Add(aggregateInfo, types = new List<Type>());
            }
            types.Add(typeof(TProjection));
            return this;
        }

        public ProjectionsInfo Build()
        {
            var projections = new List<ProjectionInfo>();
            foreach (var aggProjectionTypes in _projectionTypes)
            {
                foreach (var projectionType in aggProjectionTypes.Value)
                {
                    var projectionInfo = new ProjectionInfo(projectionType, aggProjectionTypes.Key);

                    var eventTypes = projectionType.GetTypeInfo().DeclaredMethods
                        .SelectMany(m => m.GetParameters())
                        .Select(p => p.ParameterType)
                        .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(Event)))
                        .Distinct();
                    projectionInfo.Events = eventTypes.Select(t => _languageInfo.GetEvent(t)).ToImmutableHashSet();

                    projectionInfo.Materialized = _configuration.ProjectionsToMaterialize.Contains(projectionInfo.Type);
                    projections.Add(projectionInfo);
                }
            }

            return new ProjectionsInfo(projections);
        }

        private readonly LanguageInfo _languageInfo;
        private readonly Configuration _configuration;
        private readonly Dictionary<AggregateInfo, List<Type>> _projectionTypes = new Dictionary<AggregateInfo, List<Type>>();
    }
}