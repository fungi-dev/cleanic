namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    public sealed class SagaInfo : DomainObjectInfo
    {
        public static SagaInfo Get(Type type) => (SagaInfo)Get(type, () => new SagaInfo(type));

        private SagaInfo(Type sagaType) : base(sagaType)
        {
            EnsureTermTypeCorrect<Saga>(sagaType);

            Events = Array.Empty<EventInfo>().ToImmutableHashSet();

            var eventTypes = Type.GetTypeInfo().DeclaredMethods
                .SelectMany(m => m.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => t.IsSubclassOf(typeof(Event)))
                .Distinct();
            Events = eventTypes.Select(x => EventInfo.Get(x)).ToImmutableHashSet();
        }

        // Сага может реагировать на события агрегата только внутри своего поддомена
        public IReadOnlyCollection<EventInfo> Events { get; internal set; }

        // Сага может реагировать на доменные события из других поддоменов, чтобы сынтегрироваться с ними
        //todo public IReadOnlyCollection<DomainEventInfo> DomainEvents { get; internal set; }
    }
}