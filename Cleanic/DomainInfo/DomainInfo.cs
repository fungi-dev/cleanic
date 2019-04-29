using Cleanic.Domain;
using System;
using System.Linq;
using System.Reflection;

namespace Cleanic.DomainInfo
{
    public class DomainInfo
    {
        public DomainInfo(Type[] aggregateTypes, Type[] sagaTypes, Type[] projectionTypes)
        {
            if (aggregateTypes.Any(_ => !typeof(Aggregate).GetTypeInfo().IsAssignableFrom(_.GetTypeInfo())))
            {
                throw new ArgumentException("Aggregate types are expected but some other was found!");
            }
            Aggregates = aggregateTypes.Select(x => new AggregateInfo(x)).ToArray();

            if (sagaTypes.Any(_ => !typeof(Saga).GetTypeInfo().IsAssignableFrom(_.GetTypeInfo())))
            {
                throw new ArgumentException("Saga types are expected but some other was found!");
            }
            Sagas = sagaTypes.Select(x => new SagaInfo(x)).ToArray();

            if (projectionTypes.Any(_ => !typeof(Projection).GetTypeInfo().IsAssignableFrom(_.GetTypeInfo())))
            {
                throw new ArgumentException("Projection types are expected but some other was found!");
            }
            Projections = projectionTypes.Select(x => new ProjectionInfo(x)).ToArray();
        }

        public AggregateInfo[] Aggregates { get; }

        public SagaInfo[] Sagas { get; }

        public ProjectionInfo[] Projections { get; }

        public TypeInfo FindCommand(String aggregateName, String commandName)
        {
            var aggregate = Aggregates.Single(x => String.Equals(x.Type.Name, aggregateName, StringComparison.OrdinalIgnoreCase));
            var commands = aggregate.Type.GetTypeInfo().DeclaredNestedTypes
                                    .Where(x => typeof(Command).GetTypeInfo().IsAssignableFrom(x));
            return commands.Single(x => String.Equals(x.Name, commandName, StringComparison.OrdinalIgnoreCase));
        }
    }
}