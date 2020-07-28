using System;
using System.Collections.Generic;

namespace Cleanic
{
    public class SagaInfo : DomainObjectInfo
    {
        public IReadOnlyCollection<EventInfo> Events { get; internal set; }

        public SagaInfo(Type sagaType) : base(sagaType) { }
    }
}