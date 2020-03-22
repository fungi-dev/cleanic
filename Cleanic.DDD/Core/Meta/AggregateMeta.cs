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

            var projectionTypes = nestedTypes.Where(x => x.IsProjection());
            Projections = projectionTypes.Select(x => new ProjectionMeta(x.AsType(), domain)).ToImmutableHashSet();
        }

        public IReadOnlyCollection<ProjectionMeta> Projections { get; }

        public void RunHandler(IAggregate aggregate, ICommand command)
        {
            var methods = Type.GetRuntimeMethods().Where(x => x.GetParameters().Length == 1 && x.ReturnType == typeof(void));
            var handler = methods.Single(x => x.GetParameters()[0].ParameterType == command.GetType());
            handler.Invoke(aggregate, new[] { command });
        }
    }

    public static class AggregateTypeExtensions
    {
        public static Boolean IsAggregate(this Type type) => type.GetTypeInfo().IsAggregate();
        public static Boolean IsAggregate(this TypeInfo type) => type.ImplementedInterfaces.Contains(typeof(IAggregate));
    }
}