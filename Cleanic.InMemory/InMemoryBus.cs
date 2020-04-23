using Cleanic.Application;
using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cleanic.Framework
{
    //todo do logging
    /// <summary>
    /// Synchronous message bus working in memory.
    /// </summary>
    /// <remarks>
    /// This is an embedded <see cref="IMessageBus">port</see> adapter implementation for tests and experiments.
    /// </remarks>
    public class InMemoryBus : ICommandBus, IEventBus
    {
        public InMemoryBus()
        {
            _busy = false;
            _queue = new Queue<Object>();
            _eventSubscribers = new Dictionary<Type, List<Func<Event, Task>>>();
        }

        public async Task Send(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            await Handle(command);
        }

        public void HandleCommands(Func<Command, Task> handler)
        {
            if (_commandHandler != null) throw new Exception("Handler already registered!");
            _commandHandler = handler;
        }

        public async Task Publish(Event @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            await Handle(@event);
        }

        public void ListenEvents(Type eventType, Func<Event, Task> listener)
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

                if (message is Command command)
                {
                    if (_commandHandler == null) throw BadDomainException.NoCommandHandler(type);
                    await _commandHandler(command);
                }

                if (message is Event @event)
                {
                    if (_eventSubscribers.ContainsKey(type))
                    {
                        var handlers = new List<Func<Event, Task>>(_eventSubscribers[type]);
                        foreach (var handler in handlers) await handler(@event);
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