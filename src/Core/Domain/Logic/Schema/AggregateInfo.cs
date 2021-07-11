namespace Cleanic.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    public sealed class AggregateInfo : DomainObjectInfo
    {
        public static AggregateInfo Get(Type type) => (AggregateInfo)Get(type, () => new AggregateInfo(type));

        private AggregateInfo(Type aggregateType) : base(aggregateType)
        {
            EnsureTermTypeCorrect<Aggregate>(aggregateType);

            Entity = EntityInfo.Get(Type.BaseType.GenericTypeArguments.Single());

            var eventTypes = Type.GetTypeInfo().DeclaredNestedTypes.Where(x => x.IsSubclassOf(typeof(Event)));
            Events = eventTypes.Select(x => EventInfo.Get(x)).ToImmutableHashSet();
            EnsureExactOneInitialEvent(Events);

            var commands = new List<CommandInfo>();
            var dependencies = new List<ServiceInfo>();
            _dependencies = new();
            foreach (var method in Type.GetRuntimeMethods())
            {
                var paramTypes = method.GetParameters().Select(p =>
                {
                    if (p.ParameterType.GetInterface(nameof(IEnumerable)) != null)
                    {
                        var elementType = p.ParameterType.GetElementType();
                        if (!p.ParameterType.IsArray)
                        {
                            if (p.ParameterType.GenericTypeArguments.Length != 1) throw new NotImplementedException();
                            elementType = p.ParameterType.GetGenericArguments().Single();
                        }
                        return elementType;
                    }

                    return p.ParameterType;
                });
                var cmdParams = paramTypes.Where(t => t.IsSubclassOf(typeof(Command)));
                if (cmdParams.Count() != 1) continue;

                var cmdInfo = CommandInfo.Get(cmdParams.Single());
                commands.Add(cmdInfo);

                var svcParams = paramTypes.Where(t => t.IsSubclassOf(typeof(Service)));
                if (svcParams.Any())
                {
                    var svcInfos = svcParams.Select(t => ServiceInfo.Get(t)).ToArray();
                    _dependencies.Add(cmdInfo, svcInfos);
                    dependencies.AddRange(svcInfos);
                }
            }
            Commands = commands.ToImmutableHashSet();
            Dependencies = dependencies.ToImmutableHashSet();
        }

        public EntityInfo Entity { get; }
        public IReadOnlyCollection<EventInfo> Events { get; }
        public IReadOnlyCollection<CommandInfo> Commands { get; }
        public IReadOnlyCollection<ServiceInfo> Dependencies { get; }

        public IReadOnlyCollection<ServiceInfo> GetDependencies(CommandInfo commandInfo)
        {
            if (!_dependencies.TryGetValue(commandInfo, out var deps)) deps = Array.Empty<ServiceInfo>();
            return deps.ToImmutableHashSet();
        }

        private readonly Dictionary<CommandInfo, ServiceInfo[]> _dependencies;

        private void EnsureExactOneInitialEvent(IEnumerable<EventInfo> eventInfos)
        {
            var initialEvents = eventInfos.Where(i => i.IsInitial).ToArray();

            if (initialEvents.Length == 0) throw new LogicSchemaException($"Aggregate '{Type.FullName}' has no initial events");
            if (initialEvents.Length > 1) throw new LogicSchemaException($"Aggregate '{Type.FullName}' has more than one initial events");
        }
    }
}