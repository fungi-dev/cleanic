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

            foreach (var eventInfo in _projectionsInfo.MaterializingProjections.SelectMany(p => p.Events))
            {
                _eventStore.ListenEvents(eventInfo, e => ApplyEvent(e));
            }
        }

        private async Task ApplyEvent(Event @event)
        {
            var toMaterialize = _projectionsInfo.MaterializingProjections.Where(p => p.Events.Any(m => m.Type == @event.GetType()));
            foreach (var projectionInfo in toMaterialize)
            {
                var id = projectionInfo.GetIdFromEvent(@event);
                var projection = await _projectionStore.Load(id, projectionInfo.Type);
                if (projection == null) projection = (Projection)Activator.CreateInstance(projectionInfo.Type);
                projection.Apply(@event);
                await _projectionStore.Save(projection);
            }
        }

        private readonly IEventStore _eventStore;
        private readonly IProjectionStore _projectionStore;
        private readonly ProjectionsInfo _projectionsInfo;
    }
}