using System.Collections.Generic;

namespace Cleanic.Domain
{
    /// <summary>
    /// The root of domain objects tree.
    /// Such tree representing complex object, unit of change in the domain.
    /// Every change in aggregate embodied by appropriate event.
    /// </summary>
    public interface IAggregate : IEntity
    {
        IEnumerable<IEvent> FreshChanges { get; }

        IEnumerable<ICommand> FreshCommands { get; }
    }
}