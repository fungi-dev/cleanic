namespace Cleanic.Core.Tests
{
    using Shouldly;
    using System;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Схема домена описывает так, чтобы аппликейшн мог делегировать в поддомен выполнение команды.
    /// Чтобы сконструкировать схему, нужно воспользоваться классом DomainSchemaBuilder.
    /// В билдере нужно зарегистрировать все агрегаты, саги и сервисы, а потом вызвать метод Build().
    /// Во время билда билдер проанализирует члены классов с помощью рефлекшена и выяснит все детали.
    /// </summary>
    [Collection("Sequential")]
    public class DomainTests
    {
        /// <summary>
        /// Простейший случай – пустая схема; домен без агрегатов/саг/сервисов.
        /// Бесполезно, но демонстрирует поведение билдера и схемы при запросах несуществующих терминов.
        /// </summary>
        [Fact]
        public void EmptySchema()
        {
            var language = new LanguageSchemaBuilder().Build();
            var domain = new DomainSchemaBuilder(language).Build();

            domain.Aggregates.ShouldNotBeNull();
            domain.Aggregates.ShouldBeEmpty();

            domain.Sagas.ShouldNotBeNull();
            domain.Sagas.ShouldBeEmpty();

            domain.Services.ShouldNotBeNull();
            domain.Services.ShouldBeEmpty();

            Should.Throw<ArgumentNullException>(() => domain.GetAggregate(null));
            Should.Throw<ArgumentNullException>(() => domain.GetReactingSagas(null));
        }

        /// <summary>
        /// Предметка с одним агрегатом, сагой и сервисом.
        /// </summary>
        [Fact]
        public void SimpleSchema()
        {
            var language = new LanguageSchemaBuilder().Add<DemoAgg>().Build();
            var domain = new DomainSchemaBuilder(language)
                .Add<DemoAggLogic>()
                .Add<DemoSaga>()
                .Add<DemoSvc>()
                .Build();

            domain.Aggregates.Count.ShouldBe(1);
            domain.Sagas.Count.ShouldBe(1);
            domain.Services.Count.ShouldBe(1);

            var aggInfo = language.GetAggregate(typeof(DemoAgg));
            Should.NotThrow(() => domain.GetAggregate(aggInfo));
            var aggLogicInfo = domain.GetAggregate(aggInfo);
            domain.GetAggregate(aggInfo).ShouldBe(aggLogicInfo);
            aggLogicInfo.Type.ShouldBe(typeof(DemoAggLogic));
            aggLogicInfo.Name.ShouldBe(nameof(DemoAgg));
            aggLogicInfo.Aggregate.ShouldBe(aggInfo);

            aggLogicInfo.Events.ShouldNotBeNull();
            aggLogicInfo.Events.Count.ShouldBe(1);
            var aggEventInfo = aggLogicInfo.Events.Single();
            aggEventInfo.Type.ShouldBe(typeof(DemoAggLogic.AggEvent));
            aggEventInfo.Name.ShouldBe("Cleanic.Core.Tests.DemoAggLogic.AggEvent");

            Should.NotThrow(() => domain.GetReactingSagas(aggEventInfo));
            var sagas = domain.GetReactingSagas(aggEventInfo);
            sagas.ShouldNotBeNull();
            sagas.Length.ShouldBe(1);
            var saga = sagas.Single();
            domain.Sagas.Single().ShouldBe(saga);
            saga.Name.ShouldBe("Cleanic.Core.Tests.DemoSaga");
            saga.Type.ShouldBe(typeof(DemoSaga));
            saga.AggregateEvents.ShouldNotBeNull();
            saga.AggregateEvents.Count.ShouldBe(1);
            saga.AggregateEvents.Single().ShouldBe(aggEventInfo);

            var svc = domain.Services.Single();
            svc.Name.ShouldBe("Cleanic.Core.Tests.DemoSvc");
            svc.Type.ShouldBe(typeof(DemoSvc));
        }
    }
}