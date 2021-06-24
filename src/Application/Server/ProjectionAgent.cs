namespace Cleanic.Application
{
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class ProjectionAgent
    {
        public ProjectionAgent(ProjectionSchema projectionSchema, IEventStore eventStore, IViewStore viewStore, ILogger<ProjectionAgent> logger)
        {
            _projectionSchema = projectionSchema ?? throw new ArgumentNullException(nameof(projectionSchema));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _viewStore = viewStore ?? throw new ArgumentNullException(nameof(viewStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            foreach (var projectorInfo in _projectionSchema.Projectors)
            {
                foreach (var eventInfo in projectorInfo.Events)
                {
                    _eventStore.ListenEvents(eventInfo, e => ApplyEvent(e));
                    _logger.LogTrace("'{projector}' subscribed to '{event}'", projectorInfo, eventInfo);
                }
            }
        }

        private async Task ApplyEvent(AggregateEvent @event)
        {
            foreach (var projectorInfo in _projectionSchema.Projectors)
            {
                var projector = (Projector)Activator.CreateInstance(projectorInfo.Type);

                if (projectorInfo.CreateEvents.Any(e => e.Type == @event.GetType()))
                {
                    var view = projector.CreateView(@event);
                    await _viewStore.Save(view);
                    _logger.LogTrace("'{projector}' created '{view}' with '{event}'", projectorInfo, projectorInfo.AggregateView, @event);
                    continue;
                }

                if (projectorInfo.UpdateEvents.Any(e => e.Type == @event.GetType()))
                {
                    var filter = projector.GetFilter(@event, projectorInfo.AggregateView);
                    var views = await _viewStore.Load(projectorInfo.AggregateView, filter);
                    foreach (var view in views)
                    {
                        projector.UpdateView(view, @event);
                        await _viewStore.Save(view);
                        _logger.LogTrace("'{projector}' updated '{view}' with '{event}'", projectorInfo, projectorInfo.AggregateView, @event);
                    }
                }
            }
        }

        private readonly ProjectionSchema _projectionSchema;
        private readonly IEventStore _eventStore;
        private readonly IViewStore _viewStore;
        private readonly ILogger _logger;
    }
}