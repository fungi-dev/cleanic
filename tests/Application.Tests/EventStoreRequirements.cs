namespace Cleanic.Application
{
    using Cleanic.Core;
    using Shouldly;
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    public abstract class EventStoreRequirements : IDisposable
    {
        protected EventStoreRequirements()
        {
            var languageSchema = new LanguageSchemaBuilder().Add<Agg>().Build();
            LogicSchema = new LogicSchemaBuilder(languageSchema).Add<AggLogic>().Build();
            ConnectEventStore();
        }

        /// <summary>
        /// Сохранение агрегата с одним событием приводит к появлению одного события в хранилище.
        /// </summary>
        public virtual async Task SingleEventAggregateSaving()
        {
            var id = "1";
            var events = new[] { new AggLogic.Evt() };

            await EventStore.SaveEvents(id, 0, events);

            await AssertEventStoreHasOneEvent(LogicSchema.Language.GetAggregate(typeof(Agg)));
        }

        /// <summary>
        /// При попытке сохранить событие для агрегата, неверно указав текущее количество событий, хранилище сообщает об ошибке.
        /// </summary>
        public virtual async Task OptimisticLocking()
        {
            var id = "1";
            var initialEvents = new[] { new AggLogic.Evt() };
            var moreEvents = new[] { new AggLogic.Evt() };
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
            var savedEvents = new[] { new AggLogic.Evt() };

            await EventStore.SaveEvents(id, 0, savedEvents);

            var loadedEvents = await EventStore.LoadEvents(LogicSchema.Language.GetAggregate(typeof(Agg)), id);
            loadedEvents.ShouldNotBeNull();
            loadedEvents.Length.ShouldBe(savedEvents.Length);
            loadedEvents.Single().ShouldBe(savedEvents.Single());
        }

        /// <summary>
        /// События загружаются из хранилища указанием их терминов.
        /// </summary>
        public virtual async Task LoadingByEventInfo()
        {
            var id = "1";
            var savedEvents = new[] { new AggLogic.Evt() };

            await EventStore.SaveEvents(id, 0, savedEvents);

            var loadedEvents = await EventStore.LoadEvents(new[] { LogicSchema.GetAggregateEvent(typeof(AggLogic.Evt)) });
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

        public LogicSchema LogicSchema { get; }
        public IEventStore EventStore { get; protected set; }

        [Guid("64372D54-4A07-4902-9737-2AA574137139")]
        public class Agg : IAggregate { }

        [Guid("64372D54-4A07-4902-9737-2AA574137139")]
        public class AggLogic : Aggregate<Agg>
        {
            public AggLogic(String id) : base(id) { }

            [Guid("D9E79878-7A23-44D5-B925-157E7E5F001A")]
            public class Evt : AggregateEvent { }
        }
    }
}