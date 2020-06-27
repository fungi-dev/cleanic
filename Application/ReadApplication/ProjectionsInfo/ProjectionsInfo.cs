using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Cleanic.Application
{
    public class ProjectionsInfo
    {
        public IReadOnlyCollection<ProjectionInfo> Projections { get; }

        public ProjectionsInfo(IEnumerable<ProjectionInfo> projections)
        {
            Projections = projections.ToImmutableHashSet();
        }

        public ProjectionInfo GetProjection(Type projectionType)
        {
            var info = Projections.SingleOrDefault(x => x.Type == projectionType);
            return info ?? throw new Exception($"No {projectionType.FullName} in projections");
        }

        public ProjectionInfo FindProjection(String aggregateName, String projectionName)
        {
            var a = aggregateName.ToLowerInvariant();
            var p = projectionName.ToLowerInvariant();
            var projectionInfo = Projections.SingleOrDefault(x => String.Equals(x.Aggregate.Name, a, StringComparison.OrdinalIgnoreCase) && String.Equals(x.Name, p, StringComparison.OrdinalIgnoreCase));
            return projectionInfo ?? throw new Exception($"No {projectionName} in {aggregateName} aggregate");
        }
    }
}