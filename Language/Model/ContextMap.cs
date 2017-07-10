using System.Linq;
using System.Reflection;

namespace FrogsTalks
{
    public class ContextMap
    {
        public AggregateInfo[] Aggregates { get; }

        public ProjectionInfo[] Projections { get; }

        public ContextMap(params Assembly[] domainAssemblies)
        {
            var types = domainAssemblies.Distinct().SelectMany(a => a.ExportedTypes).Select(x => x.GetTypeInfo());

            var aggregates = types.Where(x => x.ImplementedInterfaces.Contains(typeof(IAggregateRoot)));
            Aggregates = aggregates.Select(x => new AggregateInfo(x)).ToArray();

            var projections = types.Where(x => x.ImplementedInterfaces.Contains(typeof(IProjection)));
            Projections = projections.Select(x => new ProjectionInfo(x)).ToArray();
        }
    }
}