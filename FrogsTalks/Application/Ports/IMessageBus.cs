using System;
using System.Collections.Generic;
using FrogsTalks.Domain;
using FrogsTalks.UseCases;

namespace FrogsTalks.Application.Ports
{
    /// <summary>
    /// An abstract message bus.
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Send the command with hope that some handler will catch it.
        /// </summary>
        void Send(ICommand command);

        /// <summary>
        /// Register the action which will handle all instances of some type of commands.
        /// The only one action can be for each type of command.
        /// </summary>
        void HandleCommands(Action<ICommand> handler);

        /// <summary>
        /// Publish the event that will be caught by all interested subscribers.
        /// </summary>
        void Publish(IEvent @event);

        /// <summary>
        /// Register the action which will handle all instances of some type of event.
        /// All registered actions will be called when such event will take place.
        /// </summary>
        void ListenEvent(Type eventType, Action<IEvent> listener);
    }

    /// <summary>
    /// Synchronous message bus working in memory.
    /// </summary>
    /// <remarks>
    /// This is an embedded <see cref="IMessageBus">port</see> adapter implementation for tests and experiments.
    /// </remarks>
    public class InMemoryBus : IMessageBus
    {
        /// <summary>
        /// Create new instance of message bus.
        /// </summary>
        public InMemoryBus()
        {
            _busy = false;
            _queue = new Queue<Object>();
            _eventSubscribers = new Dictionary<Type, List<Action<IEvent>>>();
        }

        /// <summary>
        /// Send the command with hope that some handler will catch it.
        /// </summary>
        public void Send(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            Handle(command);
        }

        /// <summary>
        /// Publish the event that will be caught by all interested subscribers.
        /// </summary>
        public void Publish(IEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            Handle(@event);
        }

        /// <summary>
        /// Register the action which will handle all instances of some type of commands.
        /// The only one action can be for each type of command.
        /// </summary>
        public void HandleCommands(Action<ICommand> handler)
        {
            if (_commandHandler != null) throw new Exception("Handler already registered!");
            _commandHandler = handler;
        }

        /// <summary>
        /// Register the action which will handle all instances of some type of event.
        /// All registered actions will be called when such event will take place.
        /// </summary>
        public void ListenEvent(Type eventType, Action<IEvent> listener)
        {
            if (!_eventSubscribers.ContainsKey(eventType)) _eventSubscribers.Add(eventType, new List<Action<IEvent>>());
            var current = _eventSubscribers[eventType];
            if (!current.Contains(listener)) current.Add(listener);
        }

        private void Handle(Object message)
        {
            _queue.Enqueue(message);
            if (_busy) return;

            try
            {
                _busy = true;
                HandleQueue();
            }
            finally
            {
                _busy = false;
            }
        }

        private void HandleQueue()
        {
            while (true)
            {
                if (_queue.Count == 0) return;
                var message = _queue.Dequeue();
                var type = message.GetType();

                if (message is ICommand)
                {
                    if (_commandHandler == null) throw new Exception("Can't find command handler!");
                    _commandHandler((ICommand)message);
                }

                if (message is IEvent)
                {
                    if (_eventSubscribers.ContainsKey(type))
                    {
                        var handlers = new List<Action<IEvent>>(_eventSubscribers[type]);
                        foreach (var handler in handlers)
                        {
                            handler((IEvent)message);
                        }
                    }
                }
            }
        }

        private Boolean _busy;
        private readonly Queue<Object> _queue;
        private Action<ICommand> _commandHandler;
        private readonly Dictionary<Type, List<Action<IEvent>>> _eventSubscribers;
    }
}