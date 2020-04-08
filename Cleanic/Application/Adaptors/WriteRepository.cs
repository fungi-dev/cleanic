using Cleanic.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class WriteRepository : IWriteRepository
    {
        public WriteRepository(IEventStore events, DomainMeta domain)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _domain = domain ?? throw new ArgumentNullException(nameof(domain));
        }

        public async Task<Aggregate> LoadOrCreate(String id, Type type)
        {
            var aggMeta = _domain.GetAggregateMeta(type);
            var persistedEvents = await _events.LoadEvents(aggMeta.Name, id);
            var agg = (Aggregate)Activator.CreateInstance(type);
            if (!persistedEvents.Any()) agg.Id = id;
            else agg.LoadFromHistory(persistedEvents);
            return agg;
        }

        public async Task<Event[]> Save(Aggregate aggregate)
        {
            var aggMeta = _domain.GetAggregateMeta(aggregate.GetType());
            if (aggregate.ProducedEvents.Any())
            {
                var persistedVersion = Convert.ToUInt32(aggregate.Version - aggregate.ProducedEvents.Count);
                await _events.SaveEvents(aggMeta.Name, aggregate.Id, aggregate.ProducedEvents, persistedVersion);
            }
            return aggregate.ProducedEvents.ToArray();
        }

        private readonly IEventStore _events;
        private readonly DomainMeta _domain;
    }
}