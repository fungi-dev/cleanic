using Shouldly;
using Xunit;

namespace Cleanic.Application.Tests
{
    [Collection("Sequential")]
    public class AuthorizationTests
    {
        [Fact]
        public void GrantSomeAggregateActions()
        {
            var sut = new Authorization();
            sut.IncorporateGrantsFromOpenIdConnectClaims("user1", "agg:create&delete");
            sut.IsAllowed("user1", createCommand, "anyAgg").ShouldBeTrue();
            sut.IsAllowed("user1", deleteCommand, "anyAgg").ShouldBeTrue();
            sut.IsAllowed("user1", editCommand, "anyAgg").ShouldBeFalse();
            sut.IsAllowed("user2", deleteCommand, "anyAgg").ShouldBeFalse();
        }

        [Fact]
        public void GrantSomeActionsOnConcreteAggregate()
        {
            var sut = new Authorization();
            sut.IncorporateGrantsFromOpenIdConnectClaims("user1", "agg:create&delete:agg1");
            sut.IsAllowed("user1", createCommand, "agg1").ShouldBeTrue();
            sut.IsAllowed("user1", deleteCommand, "agg1").ShouldBeTrue();
            sut.IsAllowed("user1", editCommand, "agg1").ShouldBeFalse();
            sut.IsAllowed("user1", deleteCommand, "agg2").ShouldBeFalse();
            sut.IsAllowed("user2", deleteCommand, "agg1").ShouldBeFalse();
        }

        [Fact]
        public void GrantSeveralAggregates()
        {
            var sut = new Authorization();
            sut.IncorporateGrantsFromOpenIdConnectClaims("user1", "agg|anotheragg");
            sut.IsAllowed("user1", createCommand, "anyAgg").ShouldBeTrue();
            sut.IsAllowed("user1", editCommand, "anyAgg").ShouldBeTrue();
            sut.IsAllowed("user1", deleteCommand, "anyAgg").ShouldBeTrue();
            sut.IsAllowed("user1", anotherCreateCommand, "anyAgg").ShouldBeTrue();
            sut.IsAllowed("user2", createCommand, "anyAgg").ShouldBeFalse();
            sut.IsAllowed("user2", anotherCreateCommand, "anyAgg").ShouldBeFalse();
        }

        [Fact]
        public void GrantSeveralAggregatesWithIdConstraints()
        {
            var sut = new Authorization();
            sut.IncorporateGrantsFromOpenIdConnectClaims("user1", "agg::agg1&agg2|anotheragg");
            sut.IsAllowed("user1", createCommand, "agg1").ShouldBeTrue();
            sut.IsAllowed("user1", createCommand, "agg2").ShouldBeTrue();
            sut.IsAllowed("user1", anotherCreateCommand, "anyAgg").ShouldBeTrue();
            sut.IsAllowed("user1", createCommand, "agg3").ShouldBeFalse();
            sut.IsAllowed("user2", anotherCreateCommand, "anyAgg").ShouldBeFalse();
        }

        [Fact]
        public void GrantSeveralAggregatesWithActionsAndIdConstraints()
        {
            var sut = new Authorization();
            sut.IncorporateGrantsFromOpenIdConnectClaims("user1", "agg:create&delete:agg1&agg2|anotheragg");
            sut.IsAllowed("user1", createCommand, "agg1").ShouldBeTrue();
            sut.IsAllowed("user1", deleteCommand, "agg1").ShouldBeTrue();
            sut.IsAllowed("user1", createCommand, "agg2").ShouldBeTrue();
            sut.IsAllowed("user1", anotherCreateCommand, "anyAgg").ShouldBeTrue();
            sut.IsAllowed("user1", editCommand, "agg1").ShouldBeFalse();
            sut.IsAllowed("user1", createCommand, "agg3").ShouldBeFalse();
            sut.IsAllowed("user2", anotherCreateCommand, "anyAgg").ShouldBeFalse();
        }

        public AuthorizationTests()
        {
            var aggInfo = new AggregateInfo(typeof(Agg));
            createCommand = new CommandInfo(typeof(Agg.Create), aggInfo);
            editCommand = new CommandInfo(typeof(Agg.Edit), aggInfo);
            deleteCommand = new CommandInfo(typeof(Agg.Delete), aggInfo);

            var anotherAggInfo = new AggregateInfo(typeof(AnotherAgg));
            anotherCreateCommand = new CommandInfo(typeof(AnotherAgg.Create), anotherAggInfo);
        }

        private readonly CommandInfo createCommand;
        private readonly CommandInfo editCommand;
        private readonly CommandInfo deleteCommand;
        private readonly CommandInfo anotherCreateCommand;

        public class Agg
        {
            public class Create { }
            public class Edit { }
            public class Delete { }
        }

        public class AnotherAgg
        {
            public class Create { }
            public class Edit { }
            public class Delete { }
        }
    }
}