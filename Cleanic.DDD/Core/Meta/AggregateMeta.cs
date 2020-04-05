using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Cleanic.Core
{
    public class AggregateMeta : EntityMeta
    {
        public AggregateMeta(Type aggregateType, DomainFacade domain) : base(aggregateType)
        {
            var nestedTypes = Type.GetTypeInfo().DeclaredNestedTypes;

            var projectionTypes = nestedTypes.Where(x => x.Is<IProjection>());
            Projections = projectionTypes.Select(x => new ProjectionMeta(x.AsType(), domain)).ToImmutableHashSet();
        }

        public IReadOnlyCollection<ProjectionMeta> Projections { get; }
    }
}