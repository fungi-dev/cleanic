using Cleanic.Domain;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    /// <summary>
    /// An abstract message bus.
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Send the command with hope that some handler will catch it.
        /// </summary>
        Task Send(Command command);

        /// <summary>
        /// Register the action which will handle all instances of some type of commands.
        /// The only one action can be for each type of command.
        /// </summary>
        void HandleCommands(Func<Command, Task> handler);

        /// <summary>
        /// Publish the event that will be caught by all interested subscribers.
        /// </summary>
        Task Publish(Event @event);

        /// <summary>
        /// Register the action which will handle all instances of some type of event.
        /// All registered actions will be called when such event will take place.
        /// </summary>
        void ListenEvent(Type eventType, Func<Event, Task> listener);
    }
}