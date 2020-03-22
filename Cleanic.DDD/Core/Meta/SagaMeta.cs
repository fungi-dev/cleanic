using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cleanic.Core
{
    public class SagaMeta
    {
        public SagaMeta(Type sagaType, IDomainFacade domain)
        {
            Type = sagaType ?? throw new ArgumentNullException(nameof(sagaType));

            var methodsWithEventParam = Type.GetRuntimeMethods().Where(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType.IsEvent());
            _reactMethods = methodsWithEventParam.Where(x => x.ReturnType.IsCommandCollection()).ToArray();
            var events = _reactMethods.Select(x => x.GetParameters()[0].ParameterType);
            Events = events.Select(x => domain.GetEventMeta(x)).ToImmutableHashSet();
        }

        public Type Type { get; }
        public IReadOnlyCollection<EventMeta> Events { get; }

        public Task<ICommand[]> RunReaction(IEvent @event)
        {
            var reactor = _reactMethods.Single(x => x.GetParameters()[0].ParameterType == @event.GetType());
            var saga = Activator.CreateInstance(Type);
            return (Task<ICommand[]>)reactor.Invoke(saga, new[] { @event });
        }

        private readonly MethodInfo[] _reactMethods;
    }

    public static class SagaTypeExtensions
    {
        public static Boolean IsSaga(this Type type) => type.GetTypeInfo().IsSaga();
        public static Boolean IsSaga(this TypeInfo type) => type.IsSubclassOf(typeof(Saga));
    }
}