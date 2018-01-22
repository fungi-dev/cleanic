using System;
using System.Linq;
using FrogsTalks.Application.Ports;
using FrogsTalks.Domain;
using FrogsTalks.DomainInfo;

namespace FrogsTalks.Application
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
        /// <param name="projections">All projections which will be building.</param>
        public ProjectionAgent(IMessageBus bus, IProjectionsRepository db, params Type[] projections)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            _db = db ?? throw new ArgumentNullException(nameof(db));

            foreach (var p in projections.Select(x => new ProjectionInfo(x)))
            {
                foreach (var eventType in p.InfluencingEventTypes)
                {
                    bus.ListenEvent(eventType, @event => RunProjectionUpdating(p, @event));
                }
            }
        }

        private void RunProjectionUpdating(ProjectionInfo projectionInfo, IEvent @event)
        {
            var eventType = @event.GetType();

            var idExtractor = projectionInfo.GetIdFromEventExtractor(eventType);
            var projectionId = idExtractor(@event);

            var projection = _db.Load(projectionId);
            if (projection == null)
            {
                projection = (IProjection)Activator.CreateInstance(projectionInfo.Type);
                projection.Id = projectionId;
            }

            var eventApplier = projectionInfo.GetEventApplier(eventType);
            eventApplier(projection, @event);

            _db.Save(projection);
        }

        private readonly IProjectionsRepository _db;
    }
}