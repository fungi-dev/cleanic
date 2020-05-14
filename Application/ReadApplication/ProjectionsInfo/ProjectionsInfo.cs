using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Cleanic.Application
{
    public class ProjectionsInfo
    {
        public IReadOnlyCollection<ProjectionInfo> MaterializingProjections { get; }
        public IReadOnlyCollection<ProjectionInfo> OnTheFlyProjections { get; }

        public ProjectionsInfo(IEnumerable<ProjectionInfo> materializingProjections, IEnumerable<ProjectionInfo> onTheFlyProjections)
        {
            MaterializingProjections = materializingProjections.ToImmutableHashSet();
            OnTheFlyProjections = onTheFlyProjections.ToImmutableHashSet();
        }

        public ProjectionInfo GetProjection(Type projectionType)
        {
            var info = MaterializingProjections.SingleOrDefault(x => x.Type == projectionType);
            if (info == null) info = OnTheFlyProjections.SingleOrDefault(x => x.Type == projectionType);
            return info ?? throw new Exception($"No {projectionType.FullName} in projections");
        }
    }
}