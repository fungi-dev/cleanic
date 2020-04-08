using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IEventBus
    {
        /// <summary>
        /// Publish the event that will be caught by all interested subscribers.
        /// </summary>
        Task Publish(Event @event);

        /// <summary>
        /// Register the action which will handle all instances of some type of event.
        /// All registered actions will be called when such event will take place.
        /// </summary>
        void ListenEvents(Type eventType, Func<Event, Task> listener);
    }
}