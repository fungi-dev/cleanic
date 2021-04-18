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
            await HandleCommand<AggregateError>(command, dependencies);
            if (_changes.Any()) return;
            await HandleCommand<AggregateEvent>(command, dependencies);
        }

        protected override IEnumerable<Object> GetIdentityComponents()
        {
            yield return Id;
        }

        private async Task HandleCommand<TResult>(Command command, IEnumerable<Service> dependencies)
            where TResult : AggregateEvent
        {
            var cmdType = command.GetType();

            var method = typeof(TResult) == typeof(AggregateError) ? GetCheckMethod(cmdType) : GetDoMethod(cmdType);
            if (method == null) return;

            var @params = method.GetParameters();
            var args = new List<Object> { command };
            for (var i = 1; i < @params.Length; i++)
            {
                var t = @params[i].ParameterType;
                if (!t.IsArray) args.Add(dependencies.Single(x => t.GetTypeInfo().IsAssignableFrom(x.GetType().GetTypeInfo())));
                else
                {
                    t = t.GetElementType();
                    var arr = dependencies.Where(x => t.GetTypeInfo().IsAssignableFrom(x.GetType().GetTypeInfo())).ToArray();
                    var arg = Array.CreateInstance(t, arr.Length);
                    arr.CopyTo(arg, 0);
                    args.Add(arg);
                }
            }

            //todo process nul result when return type is single event
            var returnType = method.ReturnType;
            if (returnType.GetTypeInfo().IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                if (returnType.GenericTypeArguments[0].IsArray)
                {
                    foreach (var @event in await (Task<TResult[]>)method.Invoke(this, args.ToArray()))
                    {
                        Apply(@event, true);
                    }
                }
                else
                {
                    var @event = await (Task<TResult>)method.Invoke(this, args.ToArray());
                    if (@event != null) Apply(@event, true);
                }
            }
            else
            {
                if (returnType.IsArray)
                {
                    foreach (var @event in (TResult[])method.Invoke(this, args.ToArray()))
                    {
                        Apply(@event, true);
                    }
                }
                else
                {
                    var @event = (TResult)method.Invoke(this, args.ToArray());
                    if (@event != null) Apply(@event, true);
                }
            }
        }

        private MethodInfo GetCheckMethod(Type commandType)
        {
            foreach (var method in GetType().GetTypeInfo().DeclaredMethods)
            {
                if (!method.GetParameters().Any(p => p.ParameterType == commandType)) continue;
                var t = method.ReturnType;
                if (t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>)) t = t.GenericTypeArguments[0];
                if (t.IsArray) t = t.GetElementType();
                if (t == typeof(AggregateError)) return method;
            }
            return null;
        }

        private MethodInfo GetDoMethod(Type commandType)
        {
            foreach (var method in GetType().GetTypeInfo().DeclaredMethods)
            {
                if (!method.GetParameters().Any(p => p.ParameterType == commandType)) continue;
                var t = method.ReturnType;
                if (t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>)) t = t.GenericTypeArguments[0];
                if (t.IsArray) t = t.GetElementType();
                if (t == typeof(AggregateEvent) && t != typeof(AggregateError)) return method;
            }
            throw new Exception($"'{GetType().Name}' don't know how to do a '{commandType.Name}'");
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