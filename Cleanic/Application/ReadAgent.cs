using Cleanic.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    //todo do logging
    public class ReadAgent
    {
        public ReadAgent(IEventBus bus, IReadRepository db, IDomainFacade domain, Configuration cfg)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _domain = domain ?? throw new ArgumentNullException(nameof(domain));
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));

            foreach (var eventMeta in _domain.ApplyingEvents)
            {
                var projectionMetas = _domain.ApplyingEvent(eventMeta).Where(x => _cfg.ProjectionsToMaterialize.Contains(x.Type));
                if (!projectionMetas.Any()) continue;
                _bus.ListenEvents(eventMeta.Type, e => ApplyEvent(e));
            }
        }

        private async Task ApplyEvent(IEvent @event)
        {
            var projectionMetas = _domain.ApplyingEvent(new EventMeta(@event.GetType()));
            foreach (var projectionMeta in projectionMetas.Where(x => _cfg.ProjectionsToMaterialize.Contains(x.Type)))
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
        private readonly Configuration _cfg;
    }
}