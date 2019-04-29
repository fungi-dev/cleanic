﻿using Cleanic.Application;
using Cleanic.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cleanic.Framework
{
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
            _eventSubscribers = new Dictionary<Type, List<Func<Event, Task>>>();
        }

        /// <summary>
        /// Send the command with hope that some handler will catch it.
        /// </summary>
        public async Task Send(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            await Handle(command);
        }

        /// <summary>
        /// Publish the event that will be caught by all interested subscribers.
        /// </summary>
        public async Task Publish(Event @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            await Handle(@event);
        }

        /// <summary>
        /// Register the action which will handle all instances of some type of commands.
        /// The only one action can be for each type of command.
        /// </summary>
        public void HandleCommands(Func<Command, Task> handler)
        {
            if (_commandHandler != null) throw new Exception("Handler already registered!");
            _commandHandler = handler;
        }

        /// <summary>
        /// Register the action which will handle all instances of some type of event.
        /// All registered actions will be called when such event will take place.
        /// </summary>
        public void ListenEvent(Type eventType, Func<Event, Task> listener)
        {
            if (!_eventSubscribers.ContainsKey(eventType)) _eventSubscribers.Add(eventType, new List<Func<Event, Task>>());
            var current = _eventSubscribers[eventType];
            if (!current.Contains(listener)) current.Add(listener);
        }

        private async Task Handle(Object message)
        {
            _queue.Enqueue(message);
            if (_busy) return;

            try
            {
                _busy = true;
                await HandleQueue();
            }
            finally
            {
                _busy = false;
            }
        }

        private async Task HandleQueue()
        {
            while (true)
            {
                if (_queue.Count == 0) return;
                var message = _queue.Dequeue();
                var type = message.GetType();

                if (message is Command)
                {
                    if (_commandHandler == null) throw new Exception("Can't find command handler!");
                    await _commandHandler((Command)message);
                }

                if (message is Event)
                {
                    if (_eventSubscribers.ContainsKey(type))
                    {
                        var handlers = new List<Func<Event, Task>>(_eventSubscribers[type]);
                        foreach (var handler in handlers)
                        {
                            await handler((Event)message);
                        }
                    }
                }
            }
        }

        private Boolean _busy;
        private readonly Queue<Object> _queue;
        private Func<Command, Task> _commandHandler;
        private readonly Dictionary<Type, List<Func<Event, Task>>> _eventSubscribers;
    }
}