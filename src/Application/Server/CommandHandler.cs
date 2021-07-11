namespace Cleanic.Application
{
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class CommandHandler
    {
        public CommandHandler(LogicSchema logicSchema, IEventStore eventStore, ICommandBus commandBus, Func<Type, Service[]> serviceFactory, ILogger<CommandHandler> logger)
        {
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _logicSchema = logicSchema ?? throw new ArgumentNullException(nameof(logicSchema));
            _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _commandBus.HandleCommands(HandleCommand);
        }

        private readonly ICommandBus _commandBus;
        private readonly IEventStore _eventStore;
        private readonly LogicSchema _logicSchema;
        private readonly Func<Type, Service[]> _serviceFactory;
        private readonly ILogger _logger;

        private async Task HandleCommand(Command command)
        {
            var commandInfo = _logicSchema.Language.GetCommand(command.GetType());
            var aggregateInfo = _logicSchema.GetAggregate(commandInfo);
            var aggregate = (Aggregate)Activator.CreateInstance(aggregateInfo.Type);

            var persistedEvents = await _eventStore.LoadEvents(aggregateInfo.Entity, command.EntityId);
            aggregate.LoadFromHistory(persistedEvents);

            var serviceInfos = aggregateInfo.GetDependencies(commandInfo);
            await aggregate.Do(command, serviceInfos.SelectMany(x => _serviceFactory(x.Type)));

            if (aggregate.ProducedEvents.Any())
            {
                var persistedEventsCount = Convert.ToUInt32(aggregate.Version - aggregate.ProducedEvents.Count);
                await _eventStore.SaveEvents(aggregate.EntityId, persistedEventsCount, aggregate.ProducedEvents);
            }

            _logger.LogTrace("'{command}' handled", command);
        }
    }
}