using Cleanic.Core;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public abstract class EventStoreRequirements : IDisposable
    {
        protected EventStoreRequirements()
        {
            LanguageInfo = new LanguageInfoBuilder().Aggregate<Agg>().Build();
            ConnectEventStore();
        }

        /// <summary>
        /// Сохранение агрегата с одним событием приводит к появлению одного события в хранилище.
        /// </summary>
        public virtual async Task SingleEventAggregateSaving()
        {
            var id = "1";
            var events = new[] { new Agg.Evt() };

            await EventStore.SaveEvents(id, 0, events);

            await AssertEventStoreHasOneEvent(LanguageInfo.GetAggregate(typeof(Agg)));
        }

        /// <summary>
        /// При попытке сохранить событие для агрегата, неверно указав текущее количество событий, хранилище сообщает об ошибке.
        /// </summary>
        public virtual async Task OptimisticLocking()
        {
            var id = "1";
            var initialEvents = new[] { new Agg.Evt() };
            var moreEvents = new[] { new Agg.Evt() };
            var initialEventsCount = (UInt32)0;

            await EventStore.SaveEvents(id, initialEventsCount, initialEvents);

            await Should.ThrowAsync<Exception>(async () => await EventStore.SaveEvents(id, initialEventsCount, moreEvents));
        }

        /// <summary>
        /// События загружаются из хранилища по идентификатору агрегата.
        /// </summary>
        public virtual async Task LoadingById()
        {
            var id = "1";
            var savedEvents = new[] { new Agg.Evt() };

            await EventStore.SaveEvents(id, 0, savedEvents);

            var loadedEvents = await EventStore.LoadEvents(LanguageInfo.GetAggregate(typeof(Agg)), id);
            loadedEvents.ShouldNotBeNull();
            loadedEvents.Length.ShouldBe(savedEvents.Length);
            loadedEvents.Single().ShouldBe(savedEvents.Single());
        }

        /// <summary>
        /// События загружаются из хранилища указанием их терминов.
        /// </summary>
        public virtual async Task LoadingByEventMeta()
        {
            var id = "1";
            var savedEvents = new[] { new Agg.Evt() };

            await EventStore.SaveEvents(id, 0, savedEvents);

            var loadedEvents = await EventStore.LoadEvents(new[] { LanguageInfo.GetEvent(typeof(Agg.Evt)) });
            loadedEvents.ShouldNotBeNull();
            loadedEvents.Length.ShouldBe(1);
            loadedEvents.Single().ShouldBe(savedEvents.Single());
        }

        public void Dispose()
        {
            DisconnectEventStore();
        }

        public abstract void ConnectEventStore();
        public abstract void DisconnectEventStore();
        public abstract Task AssertEventStoreHasOneEvent(AggregateInfo aggregateInfo);

        public LanguageInfo LanguageInfo { get; }
        public IEventStore EventStore { get; protected set; }

        public class Agg
        {
            public class Evt : Event { }
        }
    }
}