namespace Cleanic.Framework
{
    using Cleanic.Application;
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Conventions;
    using MongoDB.Driver;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Unicode;
    using System.Threading.Tasks;

    public class MongoEventStore : IEventStore
    {
        public MongoEventStore(String connectionString, LogicSchema logicSchema, ILogger<MongoEventStore> logger)
        {
            if (String.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            var conventions = new ConventionPack
            {
                new IgnoreExtraElementsConvention(true)
            };
            ConventionRegistry.Register("Cleanic Conventions", conventions, t => true);
            _mongo = new MongoClient(connectionString);

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logicSchema = logicSchema ?? throw new ArgumentNullException(nameof(logicSchema));
            _bus = new InMemoryEventBus(_logger);
            Db = _mongo.GetDatabase("events");

            _serializationOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };
        }

        public IMongoDatabase Db { get; private set; }

        public async Task<Event[]> LoadEvents(EntityInfo entityInfo, String entityId)
        {
            if (entityInfo == null) throw new ArgumentNullException(nameof(entityInfo));
            if (String.IsNullOrWhiteSpace(entityId)) throw new ArgumentNullException(nameof(entityId));

            var collection = Db.GetCollection<BsonDocument>(entityInfo.Id);
            var filter = new BsonDocument("entityId", entityId);
            var documents = await collection.Find(filter).ToListAsync();

            var events = new List<Event>();
            foreach (var doc in documents)
            {
                var eventInfoId = doc.GetValue("eventInfoId").AsString;
                var eventType = _logicSchema.FindEvent(eventInfoId).Type;
                var eventData = doc.GetValue("eventData").AsString;
                events.Add((Event)JsonSerializer.Deserialize(eventData, eventType, _serializationOptions));
            }
            return events.ToArray();
        }

        public async Task<Event[]> LoadEvents(IEnumerable<EventInfo> eventInfos)
        {
            eventInfos = eventInfos?.ToArray();
            if (eventInfos == null || !eventInfos.Any()) throw new ArgumentNullException(nameof(eventInfos));

            var result = new List<Event>();
            foreach (var eventInfo in eventInfos)
            {
                var collection = Db.GetCollection<BsonDocument>(_logicSchema.GetAggregate(eventInfo).Id);
                var filter = new BsonDocument("eventInfoId", eventInfo.Id);
                var documents = await collection.Find(filter).ToListAsync();
                foreach (var doc in documents)
                {
                    var eventData = doc.GetValue("eventData").AsString;
                    result.Add((Event)JsonSerializer.Deserialize(eventData, eventInfo.Type, _serializationOptions));
                }
            }

            return result.OrderBy(x => x.EventOccurred).ToArray();
        }

        public async Task SaveEvents(String entityId, UInt32 expectedEventsCount, IEnumerable<Event> events)
        {
            if (entityId == null) throw new ArgumentNullException(nameof(entityId));
            events = events?.ToArray();
            if (events == null || !events.Any()) throw new ArgumentNullException(nameof(events));

            var eventInfos = events.Select(x => _logicSchema.GetEvent(x.GetType()));
            var entityInfos = eventInfos.Select(x => _logicSchema.GetAggregate(x).Entity);
            var entityInfo = entityInfos.Distinct().Single();

            var collection = Db.GetCollection<BsonDocument>(entityInfo.Id);
            var filter = new BsonDocument("entityId", entityId);
            var documents = await collection.Find(filter).ToListAsync();

            var actualEventsCount = documents.Any() ? documents.Max(x => (UInt32)x.GetValue("aggregateVersion").AsInt64) : 0;
            if (expectedEventsCount != actualEventsCount)
            {
                _logger.LogError("Can't save events for aggregate {entityName} ({entityId}), it was already changed", entityInfo.Name, entityId);
                throw new Exception($"Can't save events for aggregate {entityInfo.Name} ({entityId}), it was already changed");
            }

            var newDocuments = new List<BsonDocument>();
            foreach (var @event in events)
            {
                expectedEventsCount++;
                var eventInfo = _logicSchema.GetEvent(@event.GetType());
                var eventData = JsonSerializer.Serialize(@event, eventInfo.Type, _serializationOptions);
                var document = new BsonDocument
                {
                    { "entityId", entityId },
                    { "aggregateVersion", expectedEventsCount },
                    { "eventInfoId", eventInfo.Id },
                    { "eventMoment", @event.EventOccurred },
                    { "eventData", eventData }
                };
                newDocuments.Add(document);
            }
            await collection.InsertManyAsync(newDocuments);
            foreach (var @event in events) await _bus.Publish(@event);
        }

        public void ListenEvents(EventInfo eventInfo, Func<Event, Task> listener)
        {
            if (eventInfo == null) throw new ArgumentNullException(nameof(eventInfo));
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            _bus.ListenEvents(eventInfo.Type, listener);
        }

        public void Clear()
        {
            _mongo.DropDatabase("events");
        }

        private readonly LogicSchema _logicSchema;
        private readonly IMongoClient _mongo;
        private readonly ILogger<MongoEventStore> _logger;
        private readonly InMemoryEventBus _bus;
        private readonly JsonSerializerOptions _serializationOptions;
    }
}