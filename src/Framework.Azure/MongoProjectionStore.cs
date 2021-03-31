namespace Cleanic.Framework
{
    using Cleanic.Application;
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Conventions;
    using MongoDB.Driver;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class MongoProjectionStore : IViewStore
    {
        public MongoProjectionStore(ProjectionSchema projectionSchema, String connectionString, ILogger<MongoProjectionStore> logger)
        {
            if (String.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));

            var conventions = new ConventionPack
            {
                new IgnoreExtraElementsConvention(true)
            };
            ConventionRegistry.Register("Cleanic Conventions", conventions, t => true);
            _mongo = new MongoClient(connectionString);

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _projectionSchema = projectionSchema ?? throw new ArgumentNullException(nameof(projectionSchema));
            Db = _mongo.GetDatabase("views");
        }

        public IMongoDatabase Db { get; private set; }

        public async Task<AggregateView> Load(AggregateViewInfo aggregateViewInfo, String aggregateId)
        {
            if (aggregateViewInfo == null) throw new ArgumentNullException(nameof(aggregateViewInfo));
            if (String.IsNullOrWhiteSpace(aggregateId)) throw new ArgumentNullException(nameof(aggregateId));

            var collection = Db.GetCollection<BsonDocument>(aggregateViewInfo.FullName);
            var filter = new BsonDocument("AggregateId", aggregateId);

            var documents = await collection.Find(filter).ToListAsync();
            var document = documents.SingleOrDefault();
            if (document == null) return null;
            return (AggregateView)BsonSerializer.Deserialize(document, aggregateViewInfo.Type);
        }

        public async Task Save(AggregateView aggregateView)
        {
            if (aggregateView == null) throw new ArgumentNullException(nameof(aggregateView));

            var aggregateViewInfo = _projectionSchema.Language.GetView(aggregateView.GetType());
            var collection = Db.GetCollection<BsonDocument>(aggregateViewInfo.FullName);
            var filter = new BsonDocument("AggregateId", aggregateView.AggregateId);

            var documents = await collection.Find(filter).ToListAsync();
            var document = documents.SingleOrDefault();
            if (document != null) await collection.DeleteOneAsync(filter);
            await collection.InsertOneAsync(aggregateView.ToBsonDocument());
        }

        public void Clear()
        {
            _mongo.DropDatabase("views");
        }

        private readonly ProjectionSchema _projectionSchema;
        private readonly IMongoClient _mongo;
        private readonly ILogger<MongoProjectionStore> _logger;
    }
}