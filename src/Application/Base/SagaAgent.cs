namespace Cleanic.Application
{
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class SagaAgent
    {
        public SagaAgent(DomainSchema domainSchema, IEventStore eventStore, ICommandBus commandBus, ILogger<SagaAgent> logger)
        {
            _domainSchema = domainSchema ?? throw new ArgumentNullException(nameof(domainSchema));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            foreach (var eventInfo in _domainSchema.Sagas.SelectMany(x => x.AggregateEvents))
            {
                _eventStore.ListenEvents(eventInfo, e => ReactToEvent(e));
            }
        }

        private readonly DomainSchema _domainSchema;
        private readonly IEventStore _eventStore;
        private readonly ICommandBus _commandBus;
        private readonly ILogger _logger;

        private async Task ReactToEvent(AggregateEvent @event)
        {
            var eventInfo = _domainSchema.GetAggregateEvent(@event.GetType());
            foreach (var sagaInfo in _domainSchema.GetReactingSagas(eventInfo))
            {
                var saga = (Saga)Activator.CreateInstance(sagaInfo.Type);
                var commands = await saga.Handle(@event);
                foreach (var command in commands) await _commandBus.Send(command);
            }
        }
    }
}