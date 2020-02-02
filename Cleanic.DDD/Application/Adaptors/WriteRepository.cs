using Cleanic.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class WriteRepository : IWriteRepository
    {
        public WriteRepository(IEventStore events, IDomainFacade domain)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _domain = domain ?? throw new ArgumentNullException(nameof(domain));
        }

        public async Task<IEntity> LoadOrCreate(IIdentity id, Type type)
        {
            var agg = (IAggregate)Activator.CreateInstance(type, id);
            var aggMeta = ((DomainFacade)_domain).GetAggregateMeta(agg);
            var persistedEvents = await _events.LoadEvents(aggMeta, id);
            agg.LoadFromHistory(persistedEvents);
            return agg as IEntity;
        }

        public async Task<IEvent[]> Save(IEntity entity)
        {
            var agg = (IAggregate)entity;
            var aggMeta = ((DomainFacade)_domain).GetAggregateMeta(agg);
            if (agg.ProducedEvents.Any())
            {
                var persistedVersion = Convert.ToUInt32(agg.Version - agg.ProducedEvents.Count);
                await _events.SaveEvents(aggMeta, agg.Id, agg.ProducedEvents, persistedVersion);
            }
            return agg.ProducedEvents.ToArray();
        }

        private readonly IEventStore _events;
        private readonly IDomainFacade _domain;
    }
}