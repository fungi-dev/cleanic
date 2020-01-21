using System;
using System.Collections.Generic;

namespace Cleanic.Core
{
    public interface IDomainFacade
    {
        IReadOnlyCollection<Type> AffectingEvents { get; }

        void ModifyEntity(IEntity entity, ICommand command);

        void ApplyEvent(IEvent @event);

        ICommand[] ReactToEvent(IEvent @event);
    }
}