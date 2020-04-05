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

            _reactMethods = Type.GetRuntimeMethods()
                .Where(m => m.DeclaringType != typeof(Saga))
                .Where(m => m.Returns<ICommand>())
                .ToArray();

            Events = _reactMethods
                .Select(m => domain.GetEventMeta(m.GetParameters().Single().ParameterType))
                .ToImmutableHashSet();
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
}