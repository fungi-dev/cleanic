using System.Reflection;
using FluentAssertions;
using Xunit;

namespace FrogsTalks.Tests
{
    public class AggregateInfoTests
    {
        [Fact]
        public void ShouldExtractNameFromClass()
        {
            var subject = new AggregateInfo(typeof(EmptyAggregate).GetTypeInfo());
            subject.Name.Should().Be("Empty");
        }

        [Fact]
        public void ShouldExtractIdFromClass()
        {
            var type = typeof(EmptyAggregate).GetTypeInfo();
            var subject = new AggregateInfo(type);
            subject.Id.Should().Be(type.GUID);
        }

        [Fact]
        public void ShouldWorksForAggregateWithOneCommand()
        {
            var subject = new AggregateInfo(typeof(SimpleAggregate).GetTypeInfo());
            subject.Commands.Length.Should().Be(1);
        }

        [Fact]
        public void ShouldWorksForAggregateWithManyCommands()
        {
            var subject = new AggregateInfo(typeof(ComplexAggregate).GetTypeInfo());
            subject.Commands.Length.Should().Be(2);
        }

        [Fact]
        public void ShouldWorksForEmptyAggregate()
        {
            var subject = new AggregateInfo(typeof(EmptyAggregate).GetTypeInfo());
            subject.Commands.Should().BeEmpty();
        }
    }
}