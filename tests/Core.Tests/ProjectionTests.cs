namespace Cleanic.Core.Tests
{
    using Shouldly;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Компонент Projection строит/обновляет вьюхи на основе событий, происходящих в агрегатах.
    /// Предполагается, что пользователь при взаимодействии с приложением может делать только две вещи и делает их всегда отдельно друг от друга (см. CQRS):
    /// 1. Отправлять приложению команду, которая должна изменить текущее состояние предметки.
    /// 2. Отправлять приложению запрос, чтобы получить какую-то часть текущего состояния предметки.
    /// Так вот, компонент Projection – это core-составляющая той части приложения, которая обрабатывает запросы.
    /// Часть текущего состояния предметки – это AggregateView, то есть взгляд на агрегат с какой-то из сторон.
    /// Напомним, что вся предметка поделена на поддомены, а те в свою очередь на агрегаты.
    /// На каждый агрегат можно посмотреть с разных точек зрения в зависимости от того, какую роль играет пользователь в автоматизируемом бизнесе и какую в данный момент решает задачу.
    /// Критически важно отметить, что AggregateView – это всегда те и только те данные, которые нужны какой-то роли для принятия какого-то решения во время решения какой-то задачи.
    /// Возвращаемся к Projection, который строит эти AggregateView. Он делает это, обрабатывая каждое возникшее в агрегате событие (любое изменение в предметке возбуждает событие агрегата).
    /// Такую обработку осуществляют Projector'ы, каждый из которых строит/обновляет свой AggregateView. Projector реагирует на определённые набор событий, которые могут оказать влияние на вью.
    /// Чтобы приложение знало, на какие надо подписаться события, оно спрашивает это у ProjectionSchema, которая подаётся на вход при инициализации.
    /// Эта схема содержит все прожекторы и для каждого – типы событий, при возникновении которых надо запускать работу этого прожектора.
    /// </summary>
    [Collection("Sequential")]
    public class ProjectionTests
    {
        [Fact]
        public void EmptySchema()
        {
            var language = new LanguageSchemaBuilder().Build();
            var logic = new LogicSchemaBuilder(language).Build();
            var projection = new ProjectionSchemaBuilder(logic).Build();

            projection.Projectors.ShouldNotBeNull();
            projection.Projectors.ShouldBeEmpty();
        }

        [Fact]
        public void SimpleSchema()
        {
            var language = new LanguageSchemaBuilder().Add<DemoAgg>().Build();
            var logic = new LogicSchemaBuilder(language)
                .Add<DemoAggLogic>()
                .Add<DemoSvc>()
                .Build();
            var projection = new ProjectionSchemaBuilder(logic)
                .Add<DemoProjector>()
                .Build();

            projection.Projectors.Count.ShouldBe(1);

            var prjInfo = projection.Projectors.Single();
            prjInfo.Type.ShouldBe(typeof(DemoProjector));
            prjInfo.Name.ShouldBe(nameof(DemoProjector));
            prjInfo.Aggregate.ShouldBe(language.GetAggregate(typeof(DemoAgg)));

            prjInfo.Events.ShouldNotBeNull();
            prjInfo.Events.Count.ShouldBe(1);
            var aggEventInfo = prjInfo.Events.Single();
            aggEventInfo.Type.ShouldBe(typeof(DemoAggLogic.AggEvent));
            aggEventInfo.Name.ShouldBe("Cleanic.Core.Tests.DemoAggLogic.AggEvent");
        }
    }
}