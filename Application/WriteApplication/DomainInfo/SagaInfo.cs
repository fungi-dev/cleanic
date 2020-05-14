using System;
using System.Collections.Generic;

namespace Cleanic.Application
{
    public class SagaInfo : DomainObjectInfo
    {
        public IReadOnlyCollection<EventInfo> Events { get; internal set; }

        public SagaInfo(Type sagaType) : base(sagaType) { }
    }
}