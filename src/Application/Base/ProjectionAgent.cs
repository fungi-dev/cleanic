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

            foreach (var eventInfo in _projectionSchema.Projectors.SelectMany(p => p.Events))
            {
                _eventStore.ListenEvents(eventInfo, e => ApplyEvent(e));
            }
        }

        private async Task ApplyEvent(AggregateEvent @event)
        {
            foreach (var projectorInfo in _projectionSchema.Projectors)
            {
                if (!projectorInfo.Events.Any(i => i.Type == @event.GetType())) continue;

                var projector = (Projector)Activator.CreateInstance(projectorInfo.Type);

                var id = projector.GetId(@event);
                var view = await _viewStore.Load(projectorInfo.View, id);
                if (view == null)
                {
                    view = (AggregateView)Activator.CreateInstance(projectorInfo.View.Type);
                    view.AggregateId = id;
                }

                try
                {
                    projector.Apply(view, @event);
                }
                catch (Exception _)
                {
                    view = (AggregateView)Activator.CreateInstance(projectorInfo.View.Type);
                    view.AggregateId = id;
                    var events = await _eventStore.LoadEvents(projectorInfo.Events);
                    foreach (var e in events)
                    {
                        var idFromEvent = projector.GetId(e);
                        if (!idFromEvent.Equals(view.AggregateId)) continue;
                        projector.Apply(view, @event);
                    }
                    projector.Apply(view, @event);
                }

                await _viewStore.Save(view);
            }
        }

        private readonly ProjectionSchema _projectionSchema;
        private readonly IEventStore _eventStore;
        private readonly IViewStore _viewStore;
        private readonly ILogger _logger;
    }
}