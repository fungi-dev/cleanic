using System.Reflection;
using FluentAssertions;
using Xunit;

namespace OpenDomainModel
{
    public class ContextMapTests
    {
        [Fact]
        public void ShouldFindAllAggregates()
        {
            var subject = new ContextMap(GetType().GetTypeInfo().Assembly);
            subject.Aggregates.Length.Should().Be(3);
        }

        [Fact]
        public void ShouldFindAllProjections()
        {
            var subject = new ContextMap(GetType().GetTypeInfo().Assembly);
            subject.Projections.Length.Should().Be(1);
        }
    }
}