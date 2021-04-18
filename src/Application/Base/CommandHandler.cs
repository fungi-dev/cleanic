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
            var aggregateInfo = _logicSchema.GetAggregate(commandInfo).AggregateFromLanguage;
            var aggregateLogic = await LoadOrCreate(command.AggregateId, aggregateInfo);
            var aggregateLogicInfo = _logicSchema.GetAggregate(aggregateInfo);
            if (!aggregateLogicInfo.Dependencies.TryGetValue(commandInfo, out var serviceInfos)) serviceInfos = Array.Empty<ServiceInfo>();
            await aggregateLogic.Do(command, serviceInfos.SelectMany(x => _serviceFactory(x.Type)));
            await Save(aggregateLogic);
            _logger.LogTrace("'{command}' handled", command);
        }

        private async Task<Aggregate> LoadOrCreate(String id, AggregateInfo aggregateInfo)
        {
            var aggregateLogicInfo = _logicSchema.GetAggregate(aggregateInfo);
            var persistedEvents = await _eventStore.LoadEvents(aggregateInfo, id);
            var aggregateLogic = (Aggregate)Activator.CreateInstance(aggregateLogicInfo.Type, new[] { id });
            aggregateLogic.LoadFromHistory(persistedEvents);
            return aggregateLogic;
        }

        private async Task Save(Aggregate aggregateLogic)
        {
            if (aggregateLogic.ProducedEvents.Any())
            {
                var persistedEventsCount = Convert.ToUInt32(aggregateLogic.Version - aggregateLogic.ProducedEvents.Count);
                await _eventStore.SaveEvents(aggregateLogic.Id, persistedEventsCount, aggregateLogic.ProducedEvents);
            }
        }
    }
}