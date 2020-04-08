using Cleanic.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    //todo do logging
    public class ReadAgent
    {
        public ReadAgent(IEventBus bus, IReadRepository db, DomainMeta domain, Configuration cfg)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            if (domain == null) throw new ArgumentNullException(nameof(domain));
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));

            _projections = domain.Aggregates.SelectMany(a => a.Projections)
                .Where(p => _cfg.ProjectionsToMaterialize.Contains(p.Type))
                .ToArray();

            foreach (var eventMeta in _projections.SelectMany(p => p.Events))
            {
                _bus.ListenEvents(eventMeta.Type, e => ApplyEvent(e));
            }
        }

        private async Task ApplyEvent(Event @event)
        {
            foreach (var prjMeta in _projections.Where(p => p.Events.Any(m => m.Type == @event.GetType())))
            {
                var id = prjMeta.GetIdFromEvent(@event);
                var projection = await _db.Load(prjMeta.Type, id);
                if (projection == null) projection = (Projection)Activator.CreateInstance(prjMeta.Type);
                prjMeta.HandleEvent(projection, @event);
                await _db.Save(projection);
            }
        }

        private readonly IEventBus _bus;
        private readonly IReadRepository _db;
        private readonly Configuration _cfg;
        private readonly ProjectionMeta[] _projections;
    }
}