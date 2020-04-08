using Cleanic.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class ReadRepository : IReadRepository
    {
        public ReadRepository(IProjectionStore states, IEventStore events, DomainMeta domain, Configuration cfg)
        {
            _states = states ?? throw new ArgumentNullException(nameof(states));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _domain = domain ?? throw new ArgumentNullException(nameof(domain));
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        }

        public async Task<Projection> Load(Type type, String id)
        {
            Projection projection;
            if (_cfg.ProjectionsToMaterialize.Contains(type))
            {
                projection = await _states.Load(id, type);
            }
            else
            {
                var projectionMeta = _domain.GetProjectionMeta(type);
                if (!projectionMeta.Events.Any()) return null;
                var events = await _events.LoadEvents(projectionMeta.Events);
                if (!events.Any()) return null;
                projection = (Projection)Activator.CreateInstance(type);
                projection.AggregateId = id;
                foreach (var @event in events)
                {
                    var idFromEvent = projectionMeta.GetIdFromEvent(@event);
                    if (!idFromEvent.Equals(id)) continue;
                    projectionMeta.HandleEvent(projection, @event);
                }
            }
            return projection;
        }

        public async Task Save(Projection projection)
        {
            if (_cfg.ProjectionsToMaterialize.Contains(projection.GetType()))
            {
                await _states.Save(projection);
            }
        }

        private readonly IProjectionStore _states;
        private readonly IEventStore _events;
        private readonly DomainMeta _domain;
        private readonly Configuration _cfg;
    }
}