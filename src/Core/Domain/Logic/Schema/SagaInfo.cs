namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class SagaInfo : DomainObjectInfo
    {
        // Сага может реагировать на события агрегата только внутри своего поддомена
        public IReadOnlyCollection<AggregateEventInfo> AggregateEvents { get; internal set; }

        // Сага может реагировать на доменные события из других поддоменов, чтобы сынтегрироваться с ними
        //todo public IReadOnlyCollection<DomainEventInfo> DomainEvents { get; internal set; }

        public SagaInfo(Type sagaType) : base(sagaType, null)
        {
            if (!sagaType.GetTypeInfo().IsSubclassOf(typeof(Saga))) throw new ArgumentOutOfRangeException(nameof(sagaType));
        }
    }
}