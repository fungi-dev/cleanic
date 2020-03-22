using Cleanic.Application;
using Cleanic.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Framework
{
    public class MongoEventStore : IEventStore
    {
        public MongoEventStore(String connectionString)
        {
            _serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            _mongo = new MongoClient(connectionString);
            Db = _mongo.GetDatabase("events");
        }

        public IMongoDatabase Db { get; private set; }

        public async Task<IEvent[]> LoadEvents(String aggregateName, String aggregateId)
        {
            if (String.IsNullOrWhiteSpace(aggregateName)) throw new ArgumentNullException(nameof(aggregateName));
            if (aggregateId == null) throw new ArgumentNullException(nameof(aggregateId));

            var collection = Db.GetCollection<BsonDocument>(aggregateName);
            var filter = new BsonDocument("aggregateId", aggregateId);

            var documents = await collection.Find(filter).ToListAsync();
            var eventDatas = documents.Select(x => x.GetValue("eventData").AsString);
            var events = eventDatas.Select(x => JsonConvert.DeserializeObject<IEvent>(x, _serializerSettings));
            return events.ToArray();
        }

        public async Task<IEvent[]> LoadEvents(IEnumerable<EventMeta> eventMetas)
        {
            if (eventMetas == null) throw new ArgumentNullException(nameof(eventMetas));
            eventMetas = eventMetas.ToArray();
            if (!eventMetas.Any()) throw new ArgumentNullException(nameof(eventMetas));

            var events = new Dictionary<DateTime, IEvent>();
            foreach (var aggGroup in eventMetas.GroupBy(x => x.Entity.Name))
            {
                var collection = Db.GetCollection<BsonDocument>(aggGroup.Key);
                foreach (var eventTypeGroup in aggGroup.GroupBy(x => x.Name))
                {
                    var filter = new BsonDocument("eventType", eventTypeGroup.Key);
                    var documents = await collection.Find(filter).ToListAsync();
                    foreach (var doc in documents)
                    {
                        var moment = doc.GetValue("eventMoment").ToUniversalTime();
                        var data = doc.GetValue("eventData").AsString;
                        events.Add(moment, JsonConvert.DeserializeObject<IEvent>(data, _serializerSettings));
                    }
                }
            }

            return events.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
        }

        public async Task SaveEvents(String aggregateName, String aggregateId, IEnumerable<IEvent> events, UInt32 expectedVersion)
        {
            if (String.IsNullOrWhiteSpace(aggregateName)) throw new ArgumentNullException(nameof(aggregateName));
            if (aggregateId == null) throw new ArgumentNullException(nameof(aggregateId));
            if (events == null) throw new ArgumentNullException(nameof(events));
            events = events.ToArray();
            if (!events.Any()) throw new ArgumentNullException(nameof(events));

            var collection = Db.GetCollection<BsonDocument>(aggregateName);

            var filter = new BsonDocument("aggregateId", aggregateId);
            var documents = await collection.Find(filter).ToListAsync();
            var actualVersion = documents.Any() ? documents.Max(x => (UInt32)x.GetValue("aggregateVersion").AsInt64) : 0;
            if (expectedVersion != actualVersion) throw new Exception();

            foreach (var @event in events)
            {
                expectedVersion++;
                var eventData = JsonConvert.SerializeObject(@event, _serializerSettings);
                var document = new BsonDocument
                {
                    { "aggregateId", aggregateId },
                    { "aggregateVersion", expectedVersion },
                    { "eventType", @event.GetType().Name },
                    { "eventMoment", @event.Moment },
                    { "eventData", eventData }
                };
                await collection.InsertOneAsync(document);
            }
        }

        public void Clear()
        {
            _mongo.DropDatabase("events");
        }

        private readonly JsonSerializerSettings _serializerSettings;
        private readonly IMongoClient _mongo;
    }
}