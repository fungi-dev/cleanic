using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class ReadAgent
    {
        public ReadAgent(IEventBus bus, IDomainFacade domain)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _domain = domain;

            foreach (var eventType in domain.AffectingEvents)
            {
                _bus.ListenEvents(eventType, e => ApplyEvent(e));
            }
        }

        private Task ApplyEvent(IEvent @event)
        {
            _domain.ApplyEvent(@event);
            return Task.CompletedTask;
        }

        private readonly IEventBus _bus;
        private readonly IDomainFacade _domain;
    }
}