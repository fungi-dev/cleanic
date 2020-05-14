using Cleanic.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class CommandAgent
    {
        public CommandAgent(ICommandBus commandBus, IEventStore eventStore, LanguageInfo languageInfo, DomainInfo domainInfo, Func<Type, Service[]> serviceFactory)
        {
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _languageInfo = languageInfo ?? throw new ArgumentNullException(nameof(languageInfo));
            _domainInfo = domainInfo ?? throw new ArgumentNullException(nameof(domainInfo));
            _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));

            _commandBus.HandleCommands(HandleCommand);
        }

        private readonly ICommandBus _commandBus;
        private readonly IEventStore _eventStore;
        private readonly LanguageInfo _languageInfo;
        private readonly DomainInfo _domainInfo;
        private readonly Func<Type, Service[]> _serviceFactory;

        private async Task HandleCommand(Command command)
        {
            var commandInfo = _languageInfo.GetCommand(command.GetType());
            var aggregateInfo = commandInfo.Aggregate;
            var aggregateLogic = await LoadOrCreate(command.AggregateId, aggregateInfo);
            var aggregateLogicInfo = _domainInfo.GetAggregateLogic(aggregateInfo);
            if (!aggregateLogicInfo.Dependencies.TryGetValue(commandInfo, out var serviceInfos)) serviceInfos = Array.Empty<ServiceInfo>();
            await aggregateLogic.Do(command, serviceInfos.SelectMany(x => _serviceFactory(x.Type)));
            await Save(aggregateLogic);
        }

        private async Task<AggregateLogic> LoadOrCreate(String id, AggregateInfo aggregateInfo)
        {
            var aggregateLogicInfo = _domainInfo.GetAggregateLogic(aggregateInfo);
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