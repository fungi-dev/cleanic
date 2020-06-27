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

namespace Cleanic.Framework
{
    public class MongoProjectionStore : IProjectionStore
    {
        public MongoProjectionStore(String connectionString, ILogger<MongoProjectionStore> logger, ProjectionsInfo projectionsInfo)
        {
            if (String.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));

            var conventions = new ConventionPack
            {
                new IgnoreExtraElementsConvention(true)
            };
            ConventionRegistry.Register("Cleanic Conventions", conventions, t => true);
            _mongo = new MongoClient(connectionString);

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _projectionsInfo = projectionsInfo ?? throw new ArgumentNullException(nameof(projectionsInfo));
            Db = _mongo.GetDatabase("projections");
        }

        public IMongoDatabase Db { get; private set; }

        public async Task<Projection> Load(ProjectionInfo projectionInfo, String aggregateId)
        {
            if (projectionInfo == null) throw new ArgumentNullException(nameof(projectionInfo));
            if (String.IsNullOrWhiteSpace(aggregateId)) throw new ArgumentNullException(nameof(aggregateId));

            var collection = Db.GetCollection<BsonDocument>(projectionInfo.FullName);
            var filter = new BsonDocument("AggregateId", aggregateId);

            var documents = await collection.Find(filter).ToListAsync();
            var document = documents.SingleOrDefault();
            if (document == null) return null;
            return (Projection)BsonSerializer.Deserialize(document, projectionInfo.Type);
        }

        public async Task Save(Projection projection)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));

            var projectionInfo = _projectionsInfo.GetProjection(projection.GetType());
            var collection = Db.GetCollection<BsonDocument>(projectionInfo.FullName);
            var filter = new BsonDocument("AggregateId", projection.AggregateId);

            var documents = await collection.Find(filter).ToListAsync();
            var document = documents.SingleOrDefault();
            if (document != null) await collection.DeleteOneAsync(filter);
            await collection.InsertOneAsync(projection.ToBsonDocument());
        }

        public void Clear()
        {
            _mongo.DropDatabase("projections");
        }

        private readonly ProjectionsInfo _projectionsInfo;
        private readonly IMongoClient _mongo;
        private readonly ILogger<MongoProjectionStore> _logger;
    }
}