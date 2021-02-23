using Cleanic.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class SagaAgent
    {
        public SagaAgent(ICommandBus commandBus, IEventStore eventStore, LanguageInfo languageInfo, DomainInfo domainInfo)
        {
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _languageInfo = languageInfo ?? throw new ArgumentNullException(nameof(languageInfo));
            _domainInfo = domainInfo ?? throw new ArgumentNullException(nameof(domainInfo));

            foreach (var eventInfo in _domainInfo.Sagas.SelectMany(x => x.Events))
            {
                _eventStore.ListenEvents(eventInfo, e => ReactToEvent(e));
            }
        }

        private readonly ICommandBus _commandBus;
        private readonly IEventStore _eventStore;
        private readonly LanguageInfo _languageInfo;
        private readonly DomainInfo _domainInfo;

        private async Task ReactToEvent(Event @event)
        {
            var eventInfo = _languageInfo.GetEvent(@event.GetType());
            foreach (var sagaInfo in _domainInfo.GetReactingSagas(eventInfo))
            {
                var saga = (Saga)Activator.CreateInstance(sagaInfo.Type);
                var commands = await saga.Handle(@event);
                foreach (var command in commands) await _commandBus.Send(command);
            }
        }
    }
}