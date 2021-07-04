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
            var entityInfo = _logicSchema.GetAggregate(commandInfo).Entity;
            var aggregate = await LoadOrCreate(command.EntityId, entityInfo);
            var aggregateInfo = _logicSchema.GetAggregate(entityInfo);
            if (!aggregateInfo.Dependencies.TryGetValue(commandInfo, out var serviceInfos)) serviceInfos = Array.Empty<ServiceInfo>();
            await aggregate.Do(command, serviceInfos.SelectMany(x => _serviceFactory(x.Type)));
            await Save(aggregate);
            _logger.LogTrace("'{command}' handled", command);
        }

        private async Task<Aggregate> LoadOrCreate(String id, EntityInfo entityInfo)
        {
            var aggregateInfo = _logicSchema.GetAggregate(entityInfo);
            var persistedEvents = await _eventStore.LoadEvents(entityInfo, id);
            var aggregate = (Aggregate)Activator.CreateInstance(aggregateInfo.Type, new[] { id });
            aggregate.LoadFromHistory(persistedEvents);
            return aggregate;
        }

        private async Task Save(Aggregate aggregate)
        {
            if (aggregate.ProducedEvents.Any())
            {
                var persistedEventsCount = Convert.ToUInt32(aggregate.Version - aggregate.ProducedEvents.Count);
                await _eventStore.SaveEvents(aggregate.EntityId, persistedEventsCount, aggregate.ProducedEvents);
            }
        }
    }
}