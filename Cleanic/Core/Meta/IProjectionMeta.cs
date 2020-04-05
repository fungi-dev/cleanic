using System;
using System.Collections.Generic;

namespace Cleanic.Core
{
    public interface IProjectionMeta
    {
        Type Type { get; }
        IReadOnlyCollection<EventMeta> Events { get; }

        IIdentity GetProjectionIdFromAffectingEvent(IEvent @event);
    }
}