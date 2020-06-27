using Cleanic.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class ProjectionsAgent
    {
        public ProjectionsAgent(IEventStore eventStore, IProjectionStore projectionStore, ProjectionsInfo projectionsInfo)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _projectionStore = projectionStore ?? throw new ArgumentNullException(nameof(projectionStore));
            _projectionsInfo = projectionsInfo ?? throw new ArgumentNullException(nameof(projectionsInfo));

            foreach (var eventInfo in _projectionsInfo.Projections.Where(x => x.Materialized).SelectMany(p => p.Events))
            {
                _eventStore.ListenEvents(eventInfo, e => ApplyEvent(e));
            }
        }

        private async Task ApplyEvent(Event @event)
        {
            foreach (var projectionInfo in _projectionsInfo.Projections.Where(x => x.Materialized))
            {
                if (!projectionInfo.Events.Any(i => i.Type == @event.GetType())) continue;

                var id = projectionInfo.GetIdFromEvent(@event);
                var projection = await _projectionStore.Load(projectionInfo, id);
                if (projection == null)
                {
                    projection = (Projection)Activator.CreateInstance(projectionInfo.Type);
                    projection.AggregateId = id;
                }

                try
                {
                    projection.Apply(@event);
                }
                catch (Exception _)
                {
                    projection = (Projection)Activator.CreateInstance(projectionInfo.Type);
                    projection.AggregateId = id;
                    var events = await _eventStore.LoadEvents(projectionInfo.Events);
                    foreach (var e in events)
                    {
                        var idFromEvent = projectionInfo.GetIdFromEvent(e);
                        if (!idFromEvent.Equals(projection.AggregateId)) continue;
                        projection.Apply(e);
                    }
                    projection.Apply(@event);
                }

                await _projectionStore.Save(projection);
            }
        }

        private readonly IEventStore _eventStore;
        private readonly IProjectionStore _projectionStore;
        private readonly ProjectionsInfo _projectionsInfo;
    }
}