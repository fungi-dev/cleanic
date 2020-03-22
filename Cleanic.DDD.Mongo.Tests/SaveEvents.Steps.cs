using Cleanic.Core;
using MongoDB.Bson;
using Shouldly;
using System;
using System.Threading.Tasks;

namespace Cleanic.Framework.Tests
{
    public partial class SaveEvents : FeatureFixture
    {
        private String _aggregateId;
        private String _aggregateName;
        private IEvent[] _events;
        private UInt32 _expectedVersion;

        private void Given_there_is_aggregate_with_one_event()
        {
            _aggregateId = "TestAggId";
            _aggregateName = "TestAggName";
            _events = new[] { new Event() };
            _expectedVersion = 0;
        }

        private async Task When_repostory_save_first_event()
        {
            await SUT.SaveEvents(_aggregateName, _aggregateId, _events, _expectedVersion);
        }

        private async Task When_repostory_gives_wrong_events_count()
        {
            Given_there_is_aggregate_with_one_event();
            await When_repostory_save_first_event();
        }

        private async Task Then_db_has_collection_with_one_document()
        {
            using (var collections = await SUT.Db.ListCollectionNamesAsync())
            {
                await collections.MoveNextAsync();
                collections.Current.ShouldContain(_aggregateName);
            }
            var collection = SUT.Db.GetCollection<BsonDocument>(_aggregateName);

            var documentsCount = await collection.CountDocumentsAsync(new BsonDocument());
            documentsCount.ShouldBe(1);
        }

        private async Task Then_he_cant_save_new_one()
        {
            await Should.ThrowAsync<Exception>(async () => await SUT.SaveEvents(_aggregateName, _aggregateId, _events, _expectedVersion));
        }

        public class Event : IEvent
        {
            public IIdentity EntityId => new Identity { Value = "FakeId" };
            public DateTime Moment { get; } = DateTime.UtcNow;
        }

        public class Identity : IIdentity
        {
            public String Value { get; set; }
        }
    }
}