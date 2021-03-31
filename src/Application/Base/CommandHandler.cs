namespace Cleanic.Application
{
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class CommandHandler
    {
        public CommandHandler(DomainSchema domainSchema, IEventStore eventStore, ICommandBus commandBus, Func<Type, Service[]> serviceFactory, ILogger<CommandHandler> logger)
        {
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _domainSchema = domainSchema ?? throw new ArgumentNullException(nameof(domainSchema));
            _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _commandBus.HandleCommands(HandleCommand);
        }

        private readonly ICommandBus _commandBus;
        private readonly IEventStore _eventStore;
        private readonly DomainSchema _domainSchema;
        private readonly Func<Type, Service[]> _serviceFactory;
        private readonly ILogger _logger;

        private async Task HandleCommand(Command command)
        {
            var commandInfo = _domainSchema.Language.GetCommand(command.GetType());
            var aggregateInfo = commandInfo.Aggregate;
            var aggregateLogic = await LoadOrCreate(command.AggregateId, aggregateInfo);
            var aggregateLogicInfo = _domainSchema.GetAggregate(aggregateInfo);
            if (!aggregateLogicInfo.Dependencies.TryGetValue(commandInfo, out var serviceInfos)) serviceInfos = Array.Empty<ServiceInfo>();
            await aggregateLogic.Do(command, serviceInfos.SelectMany(x => _serviceFactory(x.Type)));
            await Save(aggregateLogic);
        }

        private async Task<AggregateLogic> LoadOrCreate(String id, AggregateInfo aggregateInfo)
        {
            var aggregateLogicInfo = _domainSchema.GetAggregate(aggregateInfo);
            var persistedEvents = await _eventStore.LoadEvents(aggregateInfo, id);
            var aggregateLogic = (AggregateLogic)Activator.CreateInstance(aggregateLogicInfo.Type, new[] { id });
            aggregateLogic.LoadFromHistory(persistedEvents);
            return aggregateLogic;
        }

        private async Task Save(AggregateLogic aggregateLogic)
        {
            if (aggregateLogic.ProducedEvents.Any())
            {
                var persistedEventsCount = Convert.ToUInt32(aggregateLogic.Version - aggregateLogic.ProducedEvents.Count);
                await _eventStore.SaveEvents(aggregateLogic.Id, persistedEventsCount, aggregateLogic.ProducedEvents);
            }
        }
    }
}