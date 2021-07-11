namespace Cleanic.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public abstract class Aggregate : DomainObject
    {
        public String EntityId { get; private set; }
        public UInt32 Version { get; private set; }

        public IReadOnlyCollection<Event> ProducedEvents => _changes.ToImmutableList();

        public void LoadFromHistory(ICollection<Event> history)
        {
            if (history == null) throw new ArgumentNullException(nameof(history));
            if (!history.Any()) return;

            var initialEvents = history.OfType<InitialEvent>();
            if (initialEvents.Count() != 1) throw new MisusingLogicException("Bad aggregate '{GetType().FullName}' state (many or no initial events), can't load it from history");
            EntityId = initialEvents.Single().EntityId;

            foreach (var @event in history) Apply(@event, false);
        }

        public async Task Do(Command command, IEnumerable<Service> dependencies)
        {
            if (command is InitialCommand)
            {
                if (Version > 0) throw new MisusingLogicException($"You can't send initial command to already initialized aggregate '{GetType().FullName}'");
                EntityId = command.EntityId;
            }

            if (command is not InitialCommand && Version == 0) throw new MisusingLogicException($"The first command sent to aggregate '{GetType().FullName}' must be initial one");

            var methods = GetType().GetTypeInfo().DeclaredMethods.Where(m => m.GetParameters().Any(p => p.ParameterType == command.GetType()));
            if (methods.Count() != 1) throw new NotImplementedException($"'{GetType().FullName}' don't know how to do a '{command.GetType().FullName}'");
            var method = methods.Single();

            var @params = method.GetParameters();
            var args = new List<Object>(@params.Length);
            foreach (var paramType in @params.Select(p => p.ParameterType))
            {
                if (paramType.IsSubclassOf(typeof(Command)))
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

                if (paramType.GetInterface(nameof(IEnumerable)) != null)
                {
                    var elemType = paramType.GetElementType();
                    if (!paramType.IsArray)
                    {
                        if (paramType.GenericTypeArguments.Length != 1) throw new LogicException($"Can't do '{command.GetType().FullName}' command, bad handler signature");
                        elemType = paramType.GetGenericArguments().Single();
                    }

                    var svcs = dependencies.Where(d => d.GetType().IsAssignableTo(elemType)).ToArray();
                    var arg = Array.CreateInstance(elemType, svcs.Length);
                    svcs.CopyTo(arg, 0);

                    args.Add(arg);
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
            yield return EntityId;
        }

        /// <summary>
        /// Add event to aggregate history (update method will be invoked if it exists).
        /// No need to fill EntityId and EventOccurred properties, they will be filled automatically.
        /// </summary>
        protected void Apply(Event @event)
        {
            Apply(@event, true);
        }

        private MethodInfo GetApplierOfConcreteEvent(Type eventType)
        {
            var methods = GetType().GetRuntimeMethods().Where(x => x.GetParameters().Length == 1);
            return methods.SingleOrDefault(x => x.GetParameters()[0].ParameterType == eventType);
        }

        private void Apply(Event @event, Boolean isFresh)
        {
            if (isFresh)
            {
                @event.EntityId = EntityId;
                @event.EventOccurred = DateTime.UtcNow;
                _changes.Add(@event);
            }
            var applier = GetApplierOfConcreteEvent(@event.GetType());
            applier?.Invoke(this, new Object[] { @event });
            Version++;
        }

        private readonly List<Event> _changes = new();
    }

    public abstract class Aggregate<T> : Aggregate where T : Entity { }
}