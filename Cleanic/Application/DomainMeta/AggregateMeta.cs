using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cleanic.Core
{
    public class AggregateMeta : DomainObjectMeta
    {
        public IReadOnlyCollection<CommandMeta> Commands { get; internal set; }
        public IReadOnlyCollection<EventMeta> Events { get; internal set; }
        public IReadOnlyCollection<ProjectionMeta> Projections { get; internal set; }

        public AggregateMeta(TypeInfo aggregateType, TypeInfo containerForAggregateMembers) : base(aggregateType)
        {
            Name = containerForAggregateMembers.Name;
        }

        public void InjectServices(IEnumerable<ServiceMeta> services)
        {
            _services = services.ToArray();
        }

        public ServiceMeta[] GetDependencies(Command command)
        {
            var cmdType = command.GetType();
            var svcTypes = Type.GetTypeInfo().DeclaredMethods
                .Where(m => m.GetParameters().Any(p => p.ParameterType == cmdType))
                .SelectMany(m => m.GetParameters())
                .Select(p => p.ParameterType.IsArray ? p.ParameterType.GetElementType() : p.ParameterType)
                .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(Service)))
                .Distinct();

            return svcTypes.SelectMany(t => _services.Where(s => t.GetTypeInfo().IsAssignableFrom(s.Type))).ToArray();
        }

        private ServiceMeta[] _services = Array.Empty<ServiceMeta>();
    }
}