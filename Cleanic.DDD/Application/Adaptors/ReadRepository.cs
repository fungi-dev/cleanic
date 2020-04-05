using Cleanic.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class ReadRepository : IReadRepository
    {
        public ReadRepository(IProjectionStore states, IEventStore events, IDomainFacade domain, Configuration cfg)
        {
            _states = states ?? throw new ArgumentNullException(nameof(states));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _domain = domain ?? throw new ArgumentNullException(nameof(domain));
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        }

        public async Task<IProjection> Load(Type type, IIdentity id)
        {
            IProjection projection;
            if (_cfg.ProjectionsToMaterialize.Contains(type))
            {
                projection = await _states.Load(id, type);
            }
            else
            {
                projection = (IProjection)Activator.CreateInstance(type);
                var projectionMeta = _domain.GetProjectionMeta(projection);
                if (!projectionMeta.Events.Any()) return null;
                var events = await _events.LoadEvents(projectionMeta.Events);
                if (!events.Any()) return null;
                foreach (var @event in events)
                {
                    var idFromEvent = projectionMeta.GetProjectionIdFromAffectingEvent(@event);
                    if (!idFromEvent.Equals(id)) continue;
                    _domain.ApplyEvent(projection, @event);
                }
            }
            return projection;
        }

        public async Task Save(IProjection projection)
        {
            if (_cfg.ProjectionsToMaterialize.Contains(projection.GetType()))
            {
                await _states.Save(projection);
            }
        }

        private readonly IProjectionStore _states;
        private readonly IEventStore _events;
        private readonly IDomainFacade _domain;
        private readonly Configuration _cfg;
    }
}