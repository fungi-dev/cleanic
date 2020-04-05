using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cleanic.Core
{
    public interface IDomainFacade
    {
        IReadOnlyCollection<EventMeta> ApplyingEvents { get; }

        IReadOnlyCollection<IProjectionMeta> ApplyingEvent(Type eventType);

        void ApplyEvent(IProjection projection, IEvent @event);

        IReadOnlyCollection<EventMeta> ReactingEvents { get; }

        Task<ICommand[]> ReactToEvent(IEvent @event);

        Task ModifyEntity(IEntity entity, ICommand command);

        CommandMeta GetCommandMeta(ICommand command);

        EventMeta GetEventMeta(IEvent @event);

        EventMeta GetEventMeta(Type eventType);

        IProjectionMeta GetProjectionMeta(IProjection projection);

        Type FindCommand(String aggregateName, String commandName);

        Type FindQuery(String aggregateName, String projectionName, String queryName);
    }
}