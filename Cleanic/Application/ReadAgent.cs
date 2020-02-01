using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    //todo do logging
    public class ReadAgent
    {
        public ReadAgent(IEventBus bus, IReadRepository db, IDomainFacade domain)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _domain = domain;

            foreach (var eventMeta in domain.ApplyingEvents)
            {
                _bus.ListenEvents(eventMeta.Type, e => ApplyEvent(e));
            }
        }

        private async Task ApplyEvent(IEvent @event)
        {
            var projectionMetas = _domain.ApplyingEvent(new EventMeta(@event.GetType()));
            foreach (var projectionMeta in projectionMetas)
            {
                var id = projectionMeta.GetProjectionIdFromAffectingEvent(@event);
                var projection = await _db.Load(projectionMeta.Type, id);
                if (projection == null) projection = (IProjection)Activator.CreateInstance(projectionMeta.Type);
                _domain.ApplyEvent(projection, @event);
                await _db.Save(projection);
            }
        }

        private readonly IEventBus _bus;
        private readonly IReadRepository _db;
        private readonly IDomainFacade _domain;
    }
}