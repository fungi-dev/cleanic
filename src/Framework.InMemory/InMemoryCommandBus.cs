namespace Cleanic.Framework
{
    using Cleanic.Application;
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class InMemoryCommandBus : ICommandBus
    {
        public InMemoryCommandBus(ILogger<InMemoryCommandBus> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _busy = false;
            _queue = new Queue<Command>();
        }

        public async Task Send(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _queue.Enqueue(command);
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

        public void HandleCommands(Func<Command, Task> handler)
        {
            if (_commandHandler != null) throw new Exception("Handler already registered!");
            _commandHandler = handler;
        }

        private async Task HandleQueue()
        {
            while (true)
            {
                if (_queue.Count == 0) return;
                var command = _queue.Dequeue();
                var type = command.GetType();

                if (_commandHandler == null) throw new Exception($"No handler for {type.FullName}");
                await _commandHandler(command);
            }
        }

        private readonly ILogger _logger;
        private Boolean _busy;
        private readonly Queue<Command> _queue;
        private Func<Command, Task> _commandHandler;
    }
}