namespace Cleanic.Core.Tests
{
    using Shouldly;
    using System;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Схема языка описывает внешнюю сторону предметки.
    /// Чтобы сконструкировать схему, нужно воспользоваться билдером.
    /// В билдере нужно зарегистрировать все класс-агрегаты и вызвать метод Build().
    /// Во время билда билдер проанализирует члены классов с помощью рефлекшена и выяснит все команды, запросы и события.
    /// </summary>
    [Collection("Sequential")]
    public class LanguageTests
    {
        /// <summary>
        /// Простейший случай – пустая схема; язык без терминов.
        /// Бесполезно, но демонстрирует поведение билдера и схемы при запросах несуществующих терминов.
        /// </summary>
        [Fact]
        public void EmptySchema()
        {
            var schema = new LanguageSchemaBuilder().Build();

            schema.Aggregates.ShouldNotBeNull();
            schema.Aggregates.ShouldBeEmpty();

            Should.Throw<ArgumentNullException>(() => schema.GetAggregate(null));
            Should.Throw<ArgumentNullException>(() => schema.GetCommand(null));
            Should.Throw<ArgumentNullException>(() => schema.GetQuery(null));

            var anyTermType = typeof(Object); // no matter what it is because language is empty
            Should.Throw<ArgumentOutOfRangeException>(() => schema.GetAggregate(anyTermType));
            Should.Throw<ArgumentOutOfRangeException>(() => schema.GetCommand(anyTermType));
            Should.Throw<ArgumentOutOfRangeException>(() => schema.GetQuery(anyTermType));

            var anyTermName = "sdfsdf"; // no matter what it is because language is empty
            Should.Throw<LanguageSchemaException>(() => schema.FindCommand(anyTermName, anyTermName));
        }

        /// <summary>
        /// Предметка с одним агрегатом, командой и вьюхой с запросом.
        /// </summary>
        [Fact]
        public void SimpleSchema()
        {
            var language = new LanguageSchemaBuilder().Add<DemoAgg>().Build();

            language.Aggregates.Count.ShouldBe(1);

            Should.NotThrow(() => language.GetAggregate(typeof(DemoAgg)));
            var aggInfo = language.GetAggregate(typeof(DemoAgg));
            language.GetAggregate(typeof(DemoAgg)).ShouldBe(aggInfo);
            aggInfo.Type.ShouldBe(typeof(DemoAgg));
            aggInfo.Name.ShouldBe(nameof(DemoAgg));

            aggInfo.Commands.ShouldNotBeNull();
            aggInfo.Commands.Count.ShouldBe(1);
            var cmdInfo = aggInfo.Commands.Single();
            language.GetCommand(typeof(DemoAgg.Cmd)).ShouldBe(cmdInfo);
            language.FindCommand("DemoAgg", "Cmd").ShouldBe(typeof(DemoAgg.Cmd));
            cmdInfo.Type.ShouldBe(typeof(DemoAgg.Cmd));
            cmdInfo.Name.ShouldBe(nameof(DemoAgg.Cmd));
            cmdInfo.Aggregate.ShouldBe(aggInfo);

            aggInfo.Views.ShouldNotBeNull();
            aggInfo.Views.Count.ShouldBe(1);
            var viewInfo = aggInfo.Views.Single();
            language.GetView(typeof(DemoAgg.View)).ShouldBe(viewInfo);
        }
    }
}