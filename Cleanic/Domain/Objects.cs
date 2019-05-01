using System.Collections.Generic;

namespace Cleanic.Domain
{
    /// <summary>
    /// The object having identity in the domain.
    /// </summary>
    public interface IEntity { }

    /// <summary>
    /// The root of domain objects tree.
    /// Such tree representing complex object, unit of change in the domain.
    /// Every change in aggregate embodied by appropriate event.
    /// </summary>
    public interface IAggregate : IEntity
    {
        IEnumerable<Event> FreshChanges { get; }

        IEnumerable<Command> FreshCommands { get; }
    }

    /// <summary>
    /// Information about the state of the domain.
    /// </summary>
    public interface IProjection : IEntity { }
}