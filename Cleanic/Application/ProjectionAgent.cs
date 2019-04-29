using Cleanic.Domain;
using Cleanic.DomainInfo;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    /// <summary>
    /// Agent behind the bus who updates domain projections when events occurred.
    /// </summary>
    /// <remarks>There can be many projection agent instances for one facade.</remarks>
    public class ProjectionAgent
    {
        /// <summary>
        /// Create an instance of the application projection agent.
        /// </summary>
        /// <param name="bus">Bus to catch events.</param>
        /// <param name="db">Place to store built projections.</param>
        /// <param name="domainInfo">Domain projections in which should be tracked.</param>
        public ProjectionAgent(IMessageBus bus, Repository db, DomainInfo.DomainInfo domainInfo)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            _db = db ?? throw new ArgumentNullException(nameof(db));

            foreach (var p in domainInfo.Projections)
            {
                foreach (var eventType in p.InfluencingEventTypes)
                {
                    bus.ListenEvent(eventType, @event => RunProjectionUpdating(p, @event));
                }
            }
        }

        private async Task RunProjectionUpdating(ProjectionInfo projectionInfo, Event e)
        {
            var eventType = e.GetType();

            var idExtractor = projectionInfo.GetIdFromEventExtractor(eventType);
            var id = idExtractor(e);

            var projection = (Projection)await _db.Load(id, projectionInfo.Type);
            if (projection == null)
            {
                projection = (Projection)Activator.CreateInstance(projectionInfo.Type, id);
            }

            var eventApplier = projectionInfo.GetEventApplier(eventType);
            eventApplier(projection, e);

            await _db.Save(projection);
        }

        private readonly Repository _db;
    }
}