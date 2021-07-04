namespace Cleanic.Application
{
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    public class SagaAgent
    {
        public SagaAgent(LogicSchema logicSchema, IEventStore eventStore, ICommandBus commandBus, ILogger<SagaAgent> logger)
        {
            _logicSchema = logicSchema ?? throw new ArgumentNullException(nameof(logicSchema));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            foreach (var sagaInfo in _logicSchema.Sagas)
            {
                foreach (var eventInfo in sagaInfo.Events)
                {
                    _eventStore.ListenEvents(eventInfo, e => ReactToEvent(e));
                    _logger.LogTrace("'{saga}' subscribed to '{event}'", sagaInfo, eventInfo);
                }
            }
        }

        private readonly LogicSchema _logicSchema;
        private readonly IEventStore _eventStore;
        private readonly ICommandBus _commandBus;
        private readonly ILogger _logger;

        private async Task ReactToEvent(Event @event)
        {
            var eventInfo = _logicSchema.GetEvent(@event.GetType());
            foreach (var sagaInfo in _logicSchema.GetReactingSagas(eventInfo))
            {
                var saga = (Saga)Activator.CreateInstance(sagaInfo.Type);
                var commands = await saga.Handle(@event);
                foreach (var command in commands)
                {
                    await _commandBus.Send(command);
                    _logger.LogTrace("'{saga}' produced '{command}' in reaction to '{event}'", sagaInfo, command, eventInfo);
                }
            }
        }
    }
}