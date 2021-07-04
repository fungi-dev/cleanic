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
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class MongoViewStore : IViewStore
    {
        public MongoViewStore(ProjectionSchema projectionSchema, String connectionString, ILogger<MongoViewStore> logger)
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

        public async Task<View> Load(ViewInfo viewInfo, String entityId)
        {
            if (viewInfo == null) throw new ArgumentNullException(nameof(viewInfo));
            if (String.IsNullOrWhiteSpace(entityId)) throw new ArgumentNullException(nameof(entityId));

            var collection = Db.GetCollection<BsonDocument>(viewInfo.Id);
            var filter = new BsonDocument("entityId", entityId);

            var documents = await collection.Find(filter).ToListAsync();
            var document = documents.SingleOrDefault();
            if (document == null) return null;
            return (View)BsonSerializer.Deserialize(document, viewInfo.Type);
        }

        public async Task<View[]> Load(ViewInfo viewInfo, Expression<Func<View, Boolean>> filterExpression)
        {
            if (viewInfo == null) throw new ArgumentNullException(nameof(viewInfo));
            if (filterExpression == null) throw new ArgumentNullException(nameof(filterExpression));

            var collection = Db.GetCollection<BsonDocument>(viewInfo.Id);
            //var filter = Builders<BsonDocument>.Filter.Eq(x => x.A, "1");
            //filter &= (Builders<User>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
            var filter = new BsonDocument();
            var documents = await collection.Find(filter).ToListAsync();
            var views = documents.Select(d => (View)BsonSerializer.Deserialize(d, viewInfo.Type));
            return views.Where(filterExpression.Compile()).ToArray();
        }

        public async Task Save(View view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));

            var viewInfo = _projectionSchema.Language.GetView(view.GetType());
            var collection = Db.GetCollection<BsonDocument>(viewInfo.Id);
            var filter = new BsonDocument("entityId", view.EntityId);

            var documents = await collection.Find(filter).ToListAsync();
            var document = documents.SingleOrDefault();
            if (document != null) await collection.DeleteOneAsync(filter);
            await collection.InsertOneAsync(view.ToBsonDocument());
        }

        public void Clear()
        {
            _mongo.DropDatabase("views");
        }

        private readonly ProjectionSchema _projectionSchema;
        private readonly IMongoClient _mongo;
        private readonly ILogger _logger;
    }
}