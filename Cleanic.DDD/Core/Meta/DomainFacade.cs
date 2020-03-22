using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Core
{
    //todo do logging
    public class DomainFacade : IDomainFacade
    {
        public DomainFacade(params Type[] aggregateOrSagaTypes)
        {
            _aggregates = aggregateOrSagaTypes.Where(x => x.IsAggregate()).Select(x => new AggregateMeta(x, this)).ToArray();
            _sagas = aggregateOrSagaTypes.Where(x => x.IsSaga()).Select(x => new SagaMeta(x, this)).ToArray();

            var projections = _aggregates.SelectMany(x => x.Projections);
            ApplyingEvents = projections.SelectMany(x => x.Events).ToImmutableHashSet();

            ReactingEvents = _sagas.SelectMany(x => x.Events).ToImmutableHashSet();
        }

        public IReadOnlyCollection<EventMeta> ApplyingEvents { get; }
        public IReadOnlyCollection<EventMeta> ReactingEvents { get; }

        //todo move to DDD-specific application layer
        public void ApplyEvent(IProjection projection, IEvent @event)
        {
            var meta = _aggregates.SelectMany(x => x.Projections).Single(x => x.Type == projection.GetType());
            meta.RunApplier(projection, @event);
        }

        //todo move to DDD-specific application layer
        public void ModifyEntity(IEntity entity, ICommand command)
        {
            var meta = _aggregates.Single(x => x.Type == entity.GetType());
            meta.RunHandler((IAggregate)entity, command);
        }

        //todo move to DDD-specific application layer
        public async Task<ICommand[]> ReactToEvent(IEvent @event)
        {
            var cmds = new List<ICommand>();
            var eventMeta = GetEventMeta(@event);
            foreach (var sagaMeta in _sagas.Where(x => x.Events.Contains(eventMeta)))
            {
                cmds.AddRange(await sagaMeta.RunReaction(@event));
            }
            return cmds.ToArray();
        }

        //todo move to DDD-specific application layer
        public IReadOnlyCollection<IProjectionMeta> ApplyingEvent(Type eventType)
        {
            var eventMeta = _aggregates.SelectMany(x => x.Events).Single(x => x.Type == eventType);
            var projections = new List<ProjectionMeta>();
            foreach (var projection in _aggregates.SelectMany(x => x.Projections))
            {
                if (projection.Events.Contains(eventMeta)) projections.Add(projection);
            }
            return projections.ToArray();
        }

        public CommandMeta GetCommandMeta(ICommand command)
        {
            return _aggregates.SelectMany(x => x.Commands).Single(x => x.Type == command.GetType());
        }

        public EventMeta GetEventMeta(IEvent @event)
        {
            return _aggregates.SelectMany(x => x.Events).Single(x => x.Type == @event.GetType());
        }

        public EventMeta GetEventMeta(Type eventType)
        {
            return _aggregates.SelectMany(x => x.Events).Single(x => x.Type == eventType);
        }

        public IProjectionMeta GetProjectionMeta(IProjection projection)
        {
            return _aggregates.SelectMany(x => x.Projections).Single(x => x.Type == projection.GetType());
        }

        public AggregateMeta GetAggregateMeta(IAggregate aggregate)
        {
            return _aggregates.Single(x => x.Type == aggregate.GetType());
        }

        private readonly AggregateMeta[] _aggregates;
        private readonly SagaMeta[] _sagas;
    }
}