namespace Cleanic.Framework.Tests
{
    using Cleanic.Application;
    using Cleanic.Core;
    using Microsoft.Extensions.Logging.Abstractions;
    using Shouldly;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("Sequential")]
    public class InMemoryEventStoreTests : EventStoreRequirements
    {
        [Fact]
        public override Task SingleEventAggregateSaving() => base.SingleEventAggregateSaving();

        [Fact]
        public override Task OptimisticLocking() => base.OptimisticLocking();

        [Fact]
        public override Task LoadingById() => base.LoadingById();

        [Fact]
        public override Task LoadingByEventMeta() => base.LoadingByEventMeta();

        public override void ConnectEventStore()
        {
            EventStore = new InMemoryEventStore(LogicSchema, NullLogger<InMemoryEventStore>.Instance);
        }

        public override void DisconnectEventStore()
        {
            EventStore = null;
        }

        public override Task AssertEventStoreHasOneEvent(AggregateInfo aggregateInfo)
        {
            var records = ((InMemoryEventStore)EventStore).Db.Where(x => x.AggregateInfo == aggregateInfo);
            records.Count().ShouldBe(1);
            return Task.CompletedTask;
        }
    }
}