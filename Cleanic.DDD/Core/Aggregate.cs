using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cleanic.Core
{
    public interface IAggregate : IEntity
    {
        UInt32 Version { get; }
        IReadOnlyCollection<IEvent> ProducedEvents { get; }

        void LoadFromHistory(ICollection<IEvent> history);
        Type[] GetDependencies(ICommand command);
        Task Do(ICommand command, IEnumerable<IDomainService> dependencies);
    }

    public abstract class Aggregate<T> : IAggregate
        where T : Entity<T>
    {
        public Aggregate(IIdentity id)
        {
            State = (T)Activator.CreateInstance(typeof(T), id);
        }

        public IIdentity Id => State?.Id;
        public T State { get; }
        public UInt32 Version { get; private set; }

        public IReadOnlyCollection<IEvent> ProducedEvents => _changes.ToImmutableList();

        public void LoadFromHistory(ICollection<IEvent> history)
        {
            foreach (var e in history)
            {
                var stateOfEventType = e.GetType().GetTypeInfo().BaseType.GenericTypeArguments.Single();
                if (stateOfEventType != typeof(T)) throw new Exception("Attempt to apply foreign events to aggregate!");
            }

            foreach (var @event in history) Apply(@event, false);
        }

        public Type[] GetDependencies(ICommand command)
        {
            var cmdType = command.GetType();
            var svcTypes = new HashSet<Type>();

            var checkMethod = GetCheckMethod(cmdType);
            if (checkMethod != null)
            {
                var checkParams = checkMethod.GetParameters();
                for (var i = 1; i < checkParams.Length; i++)
                {
                    var t = checkParams[i].ParameterType;
                    if (t.IsArray) t = t.GetElementType();
                    svcTypes.Add(t);
                }
            }

            var doMethod = GetDoMethod(cmdType);
            var doParams = doMethod.GetParameters();
            for (var i = 1; i < doParams.Length; i++)
            {
                var t = doParams[i].ParameterType;
                if (t.IsArray) t = t.GetElementType();
                svcTypes.Add(t);
            }

            return svcTypes.ToArray();
        }

        public async Task Do(ICommand command, IEnumerable<IDomainService> dependencies)
        {
            await HandleCommand<Error<T>>(command, dependencies);
            if (_changes.Any()) return;
            await HandleCommand<Event<T>>(command, dependencies);
        }

        private async Task HandleCommand<TResult>(ICommand command, IEnumerable<IDomainService> dependencies)
            where TResult : IEvent
        {
            var cmdType = command.GetType();

            var method = typeof(TResult).Is<IError>() ? GetCheckMethod(cmdType) : GetDoMethod(cmdType);
            if (method == null) return;

            var @params = method.GetParameters();
            var args = new List<Object> { command };
            for (var i = 1; i < @params.Length; i++)
            {
                var t = @params[i].ParameterType;
                if (!t.IsArray) args.Add(dependencies.Single(x => x.GetType() == t));
                else
                {
                    t = t.GetElementType();
                    var arr = dependencies.Where(x => t.GetTypeInfo().IsAssignableFrom(x.GetType().GetTypeInfo())).ToArray();
                    var arg = Array.CreateInstance(t, arr.Length);
                    arr.CopyTo(arg, 0);
                    args.Add(arg);
                }
            }
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
            var all = GetType().GetRuntimeMethods()
                .Where(m => m.DeclaringType != typeof(Aggregate<>))
                .Where(m => m.Returns<IError>());
            return all.SingleOrDefault(m => m.GetParameters().Any(p => p.ParameterType == commandType));
        }

        private MethodInfo GetDoMethod(Type commandType)
        {
            var all = GetType().GetRuntimeMethods()
                .Where(m => m.DeclaringType != typeof(Aggregate<>))
                .Where(m => m.Returns<IEvent>() && !m.Returns<IError>());
            var one = all.SingleOrDefault(m => m.GetParameters().Any(p => p.ParameterType == commandType));
            if (one == null) throw new Exception($"'{GetType().Name}' don't know how to do a '{commandType.Name}'");

            return one;
        }

        private MethodInfo GetApplierOfConcreteEvent(Type eventType)
        {
            var methods = typeof(T).GetRuntimeMethods().Where(x => x.GetParameters().Length == 1);
            return methods.SingleOrDefault(x => x.GetParameters()[0].ParameterType == eventType);
        }

        private void Apply(IEvent @event, Boolean isFresh)
        {
            var applier = GetApplierOfConcreteEvent(@event.GetType());
            applier?.Invoke(State, new Object[] { @event });
            Version++;

            if (isFresh) _changes.Add(@event);
        }

        private readonly List<IEvent> _changes = new List<IEvent>();
    }
}