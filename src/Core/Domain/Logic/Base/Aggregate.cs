namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public abstract class Aggregate : DomainObject
    {
        public String Id { get; }
        public UInt32 Version { get; private set; }

        public Aggregate(String id)
        {
            Id = id;
        }

        public IReadOnlyCollection<AggregateEvent> ProducedEvents => _changes.ToImmutableList();

        public void LoadFromHistory(ICollection<AggregateEvent> history)
        {
            foreach (var @event in history) Apply(@event, false);
        }

        public async Task Do(Command command, IEnumerable<Service> dependencies)
        {
            var methods = GetType().GetTypeInfo().DeclaredMethods.Where(m => m.GetParameters().Any(p => p.ParameterType == command.GetType()));
            if (methods.Count() != 1) throw new Exception($"'{GetType().FullName}' don't know how to do a '{command.GetType().FullName}'");
            var method = methods.Single();

            var @params = method.GetParameters();
            var args = new List<Object>(@params.Length);
            foreach (var paramType in @params.Select(p => p.ParameterType))
            {
                if (paramType == typeof(Command))
                {
                    args.Add(command);
                    continue;
                }

                if (paramType.IsSubclassOf(typeof(Service)))
                {
                    var svcs = dependencies.Where(d => d.GetType().IsAssignableTo(paramType));
                    if (!svcs.Any()) throw new LogicException($"Con't do '{command.GetType().FullName}' command, no '{paramType.FullName}' service");
                    if (svcs.Count() > 1) throw new LogicException($"Con't do '{command.GetType().FullName}' command, multiple '{paramType.FullName}' services");

                    args.Add(svcs.Single());
                    continue;
                }

                if (paramType.IsGenericType && paramType.GenericTypeArguments.Length == 1)
                {
                    var svcs = dependencies.Where(d => d.GetType().IsAssignableTo(paramType.GenericTypeArguments[0]));
                    args.Add(svcs);
                    continue;
                }

                throw new LogicException($"Can't do '{command.GetType().FullName}' command, bad handler signature");
            }

            if (method.ReturnType == typeof(Task))
            {
                await (Task)method.Invoke(this, args.ToArray());
                return;
            }

            if (method.ReturnType == typeof(void))
            {
                method.Invoke(this, args.ToArray());
                return;
            }

            throw new LogicException($"Can't do '{command.GetType().FullName}' command, bad handler signature");
        }

        protected override IEnumerable<Object> GetIdentityComponents()
        {
            yield return Id;
        }

        protected void Apply(AggregateEvent @event)
        {
            Apply(@event, true);
        }

        private MethodInfo GetApplierOfConcreteEvent(Type eventType)
        {
            var methods = GetType().GetRuntimeMethods().Where(x => x.GetParameters().Length == 1);
            return methods.SingleOrDefault(x => x.GetParameters()[0].ParameterType == eventType);
        }

        private void Apply(AggregateEvent @event, Boolean isFresh)
        {
            if (isFresh)
            {
                @event.AggregateId = Id;
                @event.EventOccurred = DateTime.UtcNow;
                _changes.Add(@event);
            }
            var applier = GetApplierOfConcreteEvent(@event.GetType());
            applier?.Invoke(this, new Object[] { @event });
            Version++;
        }

        private readonly List<AggregateEvent> _changes = new List<AggregateEvent>();
    }

    public abstract class Aggregate<T> : Aggregate
        where T : IAggregate
    {
        public Aggregate(String id) : base(id) { }
    }
}