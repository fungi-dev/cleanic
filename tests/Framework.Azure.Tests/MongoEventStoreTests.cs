namespace Cleanic.Framework.Tests
{
    using Cleanic.Application;
    using Cleanic.Core;
    using Microsoft.Extensions.Logging.Abstractions;
    using MongoDB.Bson;
    using Shouldly;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("Sequential")]
    public class MongoEventStoreTests : EventStoreRequirements
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
            EventStore = new MongoEventStore("mongodb+srv://admin:ZOeEiJReeSp1V7nO@alfacontext-tests-hhe15.azure.mongodb.net/test?retryWrites=true&w=majority", DomainSchema, NullLogger<MongoEventStore>.Instance);
        }

        public override void DisconnectEventStore()
        {
            ((MongoEventStore)EventStore).Clear();
        }

        public override async Task AssertEventStoreHasOneEvent(AggregateInfo aggregateInfo)
        {
            var collectionName = aggregateInfo.FullName;
            using (var collections = await ((MongoEventStore)EventStore).Db.ListCollectionNamesAsync())
            {
                await collections.MoveNextAsync();
                collections.Current.ShouldContain(collectionName);
            }
            var collection = ((MongoEventStore)EventStore).Db.GetCollection<BsonDocument>(collectionName);

            var documentsCount = await collection.CountDocumentsAsync(new BsonDocument());
            documentsCount.ShouldBe(1);
        }
    }
}