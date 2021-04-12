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

        public async Task<AggregateEvent[]> LoadEvents(AggregateInfo aggregateInfo, String aggregateId)
        {
            if (aggregateInfo == null) throw new ArgumentNullException(nameof(aggregateInfo));
            if (String.IsNullOrWhiteSpace(aggregateId)) throw new ArgumentNullException(nameof(aggregateId));

            var collection = Db.GetCollection<BsonDocument>(aggregateInfo.Id);
            var filter = new BsonDocument("aggregateId", aggregateId);
            var documents = await collection.Find(filter).ToListAsync();

            var events = new List<AggregateEvent>();
            foreach (var doc in documents)
            {
                var eventInfoId = doc.GetValue("eventInfoId").AsString;
                var eventType = _logicSchema.FindAggregateEvent(eventInfoId).Type;
                var eventData = doc.GetValue("eventData").AsString;
                events.Add((AggregateEvent)JsonSerializer.Deserialize(eventData, eventType, _serializationOptions));
            }
            return events.ToArray();
        }

        public async Task<AggregateEvent[]> LoadEvents(IEnumerable<AggregateEventInfo> eventInfos)
        {
            eventInfos = eventInfos?.ToArray();
            if (eventInfos == null || !eventInfos.Any()) throw new ArgumentNullException(nameof(eventInfos));

            var result = new List<AggregateEvent>();
            foreach (var eventInfo in eventInfos)
            {
                var collection = Db.GetCollection<BsonDocument>(_logicSchema.GetAggregate(eventInfo).Id);
                var filter = new BsonDocument("eventInfoId", eventInfo.Id);
                var documents = await collection.Find(filter).ToListAsync();
                foreach (var doc in documents)
                {
                    var eventData = doc.GetValue("eventData").AsString;
                    result.Add((AggregateEvent)JsonSerializer.Deserialize(eventData, eventInfo.Type, _serializationOptions));
                }
            }

            return result.OrderBy(x => x.EventOccurred).ToArray();
        }

        public async Task SaveEvents(String aggregateId, UInt32 expectedEventsCount, IEnumerable<AggregateEvent> events)
        {
            if (aggregateId == null) throw new ArgumentNullException(nameof(aggregateId));
            events = events?.ToArray();
            if (events == null || !events.Any()) throw new ArgumentNullException(nameof(events));

            var eventInfos = events.Select(x => _logicSchema.GetAggregateEvent(x.GetType()));
            var aggregateLogicInfos = eventInfos.Select(x => _logicSchema.GetAggregate(x).AggregateFromLanguage);
            var aggregateInfo = aggregateLogicInfos.Distinct().Single();

            var collection = Db.GetCollection<BsonDocument>(aggregateInfo.Id);
            var filter = new BsonDocument("aggregateId", aggregateId);
            var documents = await collection.Find(filter).ToListAsync();

            var actualEventsCount = documents.Any() ? documents.Max(x => (UInt32)x.GetValue("aggregateVersion").AsInt64) : 0;
            if (expectedEventsCount != actualEventsCount)
            {
                _logger.LogError("Can't save events for aggregate {aggregateName} ({aggregateId}), it was already changed", aggregateInfo.Name, aggregateId);
                throw new Exception($"Can't save events for aggregate {aggregateInfo.Name} ({aggregateId}), it was already changed");
            }

            var newDocuments = new List<BsonDocument>();
            foreach (var @event in events)
            {
                expectedEventsCount++;
                var eventInfo = _logicSchema.GetAggregateEvent(@event.GetType());
                var eventData = JsonSerializer.Serialize(@event, eventInfo.Type, _serializationOptions);
                var document = new BsonDocument
                {
                    { "aggregateId", aggregateId },
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

        public void ListenEvents(AggregateEventInfo eventInfo, Func<AggregateEvent, Task> listener)
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