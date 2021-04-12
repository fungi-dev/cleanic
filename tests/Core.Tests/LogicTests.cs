namespace Cleanic.Core.Tests
{
    using Shouldly;
    using System;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Схема домена описывает так, чтобы аппликейшн мог делегировать в поддомен выполнение команды.
    /// Чтобы сконструкировать схему, нужно воспользоваться классом LogicSchemaBuilder.
    /// В билдере нужно зарегистрировать все агрегаты, саги и сервисы, а потом вызвать метод Build().
    /// Во время билда билдер проанализирует члены классов с помощью рефлекшена и выяснит все детали.
    /// </summary>
    [Collection("Sequential")]
    public class LogicTests
    {
        /// <summary>
        /// Простейший случай – пустая схема; домен без агрегатов/саг/сервисов.
        /// Бесполезно, но демонстрирует поведение билдера и схемы при запросах несуществующих терминов.
        /// </summary>
        [Fact]
        public void EmptySchema()
        {
            var language = new LanguageSchemaBuilder().Build();
            var logic = new LogicSchemaBuilder(language).Build();

            logic.Aggregates.ShouldNotBeNull();
            logic.Aggregates.ShouldBeEmpty();

            logic.Sagas.ShouldNotBeNull();
            logic.Sagas.ShouldBeEmpty();

            logic.Services.ShouldNotBeNull();
            logic.Services.ShouldBeEmpty();

            Should.Throw<ArgumentNullException>(() => logic.GetReactingSagas(null));
        }

        /// <summary>
        /// Предметка с одним агрегатом, сагой и сервисом.
        /// </summary>
        [Fact]
        public void SimpleSchema()
        {
            var language = new LanguageSchemaBuilder().Add<DemoAgg>().Build();
            var logic = new LogicSchemaBuilder(language)
                .Add<DemoAggLogic>()
                .Add<DemoSaga>()
                .Add<DemoSvc>()
                .Build();

            logic.Aggregates.Count.ShouldBe(1);
            logic.Sagas.Count.ShouldBe(1);
            logic.Services.Count.ShouldBe(1);

            var aggInfo = language.GetAggregate(typeof(DemoAgg));
            Should.NotThrow(() => logic.GetAggregate(aggInfo));
            var aggLogicInfo = logic.GetAggregate(aggInfo);
            logic.GetAggregate(aggInfo).ShouldBe(aggLogicInfo);
            aggLogicInfo.Type.ShouldBe(typeof(DemoAggLogic));
            aggLogicInfo.Name.ShouldBe(nameof(DemoAgg));

            aggLogicInfo.Events.ShouldNotBeNull();
            aggLogicInfo.Events.Count.ShouldBe(1);
            var aggEventInfo = aggLogicInfo.Events.Single();
            aggEventInfo.Type.ShouldBe(typeof(DemoAggLogic.AggEvent));
            aggEventInfo.Name.ShouldBe(nameof(DemoAggLogic.AggEvent));

            Should.NotThrow(() => logic.GetReactingSagas(aggEventInfo));
            var sagas = logic.GetReactingSagas(aggEventInfo);
            sagas.ShouldNotBeNull();
            sagas.Length.ShouldBe(1);
            var saga = sagas.Single();
            logic.Sagas.Single().ShouldBe(saga);
            saga.Type.ShouldBe(typeof(DemoSaga));
            saga.Name.ShouldBe(nameof(DemoSaga));
            saga.AggregateEvents.ShouldNotBeNull();
            saga.AggregateEvents.Count.ShouldBe(1);
            saga.AggregateEvents.Single().ShouldBe(aggEventInfo);

            var svc = logic.Services.Single();
            svc.Type.ShouldBe(typeof(DemoSvc));
            svc.Name.ShouldBe(nameof(DemoSvc));
        }

        /// <summary>
        /// В схеме есть сага, которая реагирует на ивенты, генерируемые каким-то агрегатом, не объявленном в этой схеме.
        /// Это может быть, если собирается отдельное приложение для запуска саг: в схеме только саги, без агрегатов.
        /// А какое-то другое приложение собрано с агрегатами, но без саг. Оба подключены к одному ивент-стору.
        /// </summary>
        [Fact]
        public void SagaTriggeredByOutsideAggregate()
        {
            var language = new LanguageSchemaBuilder().Add<DemoAgg>().Build();
            var domain = new LogicSchemaBuilder(language).Add<DemoSaga>().Build();

            domain.Aggregates.ShouldBeEmpty();
            domain.Sagas.Count.ShouldBe(1);
            domain.Services.ShouldBeEmpty();

            var saga = domain.Sagas.Single();
            saga.AggregateEvents.Count.ShouldBe(1);
            var evnt = saga.AggregateEvents.Single();
            evnt.Type.ShouldBe(typeof(DemoAggLogic.AggEvent));
        }
    }
}