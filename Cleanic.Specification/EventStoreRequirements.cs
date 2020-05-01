using Cleanic.Application;
using Cleanic.Core;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Specification
{
    public abstract class EventStoreRequirements : IDisposable
    {
        protected EventStoreRequirements()
        {
            Domain = new DomainMetaBuilder().Aggregate<Agg>().Build();
            ConnectEventStore();
        }

        /// <summary>
        /// Сохранение агрегата с одним событием приводит к появлению одного события в хранилище.
        /// </summary>
        public virtual async Task SingleEventAggregateSaving()
        {
            var aggregate = new AggImpl { Id = "Agg" };
            await aggregate.Do(new Agg.Cmd(), Array.Empty<Service>());

            await EventStore.SaveEvents(aggregate.Id, aggregate.Version - 1, aggregate.ProducedEvents);

            await AssertEventStoreHasOneEvent(Domain.GetAggregateMeta(aggregate.GetType()));
        }

        /// <summary>
        /// При попытке сохранить событие для агрегата, неверно указав текущее количество событий, хранилище сообщает об ошибке.
        /// </summary>
        public virtual async Task OptimisticLocking()
        {
            var aggregate = new AggImpl { Id = "Agg" };
            await aggregate.Do(new Agg.Cmd(), Array.Empty<Service>());
            var freezedAggVersion = aggregate.Version - 1;

            await EventStore.SaveEvents(aggregate.Id, freezedAggVersion, aggregate.ProducedEvents);

            await Should.ThrowAsync<Exception>(async () => await EventStore.SaveEvents(aggregate.Id, freezedAggVersion, aggregate.ProducedEvents));
        }

        /// <summary>
        /// События загружаются из хранилища по идентификатору агрегата.
        /// </summary>
        public virtual async Task LoadingById()
        {
            var aggregate = new AggImpl { Id = "Agg" };
            await aggregate.Do(new Agg.Cmd(), Array.Empty<Service>());

            await EventStore.SaveEvents(aggregate.Id, aggregate.Version - 1, aggregate.ProducedEvents);

            var events = await EventStore.LoadEvents(Domain.GetAggregateMeta(aggregate.GetType()), aggregate.Id);
            events.ShouldNotBeNull();
            events.Length.ShouldBe(1);
            events.Single().ShouldBe(aggregate.ProducedEvents.Single());
        }

        /// <summary>
        /// События загружаются из хранилища указанием их мет.
        /// </summary>
        public virtual async Task LoadingByEventMeta()
        {
            var aggregate = new AggImpl { Id = "Agg" };
            await aggregate.Do(new Agg.Cmd(), Array.Empty<Service>());

            await EventStore.SaveEvents(aggregate.Id, aggregate.Version - 1, aggregate.ProducedEvents);

            var events = await EventStore.LoadEvents(new[] { Domain.GetEventMeta(typeof(Agg.Evt)) });
            events.ShouldNotBeNull();
            events.Length.ShouldBe(1);
            events.Single().ShouldBe(aggregate.ProducedEvents.Single());
        }

        public void Dispose()
        {
            DisconnectEventStore();
        }

        public abstract void ConnectEventStore();
        public abstract void DisconnectEventStore();
        public abstract Task AssertEventStoreHasOneEvent(AggregateMeta aggregateMeta);

        public DomainMeta Domain { get; }
        public IEventStore EventStore { get; protected set; }

        public class Agg
        {
            public class Cmd : Command { }
            public class Evt : Event { }
        }
        public class AggImpl : Aggregate<Agg>
        {
            public Event Do(Agg.Cmd _) => new Agg.Evt();
        }
    }
}