using Cleanic.Application;
using Cleanic.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cleanic.Framework
{
    public class MongoEventStore : IEventStore
    {
        public MongoEventStore(String connectionString, ILogger<MongoEventStore> logger, DomainMeta domain)
        {
            _mongo = new MongoClient(connectionString);
            _logger = logger;
            _domain = domain;
            Db = _mongo.GetDatabase("events");
        }

        public IMongoDatabase Db { get; private set; }

        public async Task<Event[]> LoadEvents(AggregateMeta aggregateMeta, String aggregateId)
        {
            if (aggregateMeta == null) throw new ArgumentNullException(nameof(aggregateMeta));
            if (String.IsNullOrWhiteSpace(aggregateId)) throw new ArgumentNullException(nameof(aggregateId));

            var collection = Db.GetCollection<BsonDocument>(aggregateMeta.Name);
            var filter = new BsonDocument("aggregateId", aggregateId);
            var documents = await collection.Find(filter).ToListAsync();

            var events = new List<Event>();
            foreach (var doc in documents)
            {
                var eventTypeName = doc.GetValue("eventType").AsString;
                var eventType = _domain.GetEventMeta(eventTypeName).Type;
                var eventData = doc.GetValue("eventData").AsString;
                events.Add((Event)JsonSerializer.Deserialize(eventData, eventType));
            }
            return events.ToArray();
        }

        public async Task<Event[]> LoadEvents(IEnumerable<EventMeta> eventMetas)
        {
            eventMetas = eventMetas?.ToArray();
            if (eventMetas == null || !eventMetas.Any()) throw new ArgumentNullException(nameof(eventMetas));

            var events = new List<Event>();
            foreach (var aggGroup in eventMetas.GroupBy(x => x.Aggregate.Name))
            {
                var collection = Db.GetCollection<BsonDocument>(aggGroup.Key);
                foreach (var eventTypeGroup in aggGroup.GroupBy(x => x.Name))
                {
                    var filter = new BsonDocument("eventType", eventTypeGroup.Key);
                    var documents = await collection.Find(filter).ToListAsync();
                    foreach (var doc in documents)
                    {
                        var eventTypeName = doc.GetValue("eventType").AsString;
                        var eventType = _domain.GetEventMeta(eventTypeName).Type;
                        var eventData = doc.GetValue("eventData").AsString;
                        events.Add((Event)JsonSerializer.Deserialize(eventData, eventType));
                    }
                }
            }

            return events.OrderBy(x => x.EventOccurred).ToArray();
        }

        public async Task SaveEvents(String aggregateId, UInt32 expectedEventsCount, IEnumerable<Event> aggregateEvents)
        {
            if (aggregateId == null) throw new ArgumentNullException(nameof(aggregateId));
            aggregateEvents = aggregateEvents?.ToArray();
            if (aggregateEvents == null || !aggregateEvents.Any()) throw new ArgumentNullException(nameof(aggregateEvents));
            var aggregateMeta = aggregateEvents.Select(x => _domain.GetEventMeta(x.GetType()).Aggregate).Distinct().Single();

            var collection = Db.GetCollection<BsonDocument>(aggregateMeta.Name);
            var filter = new BsonDocument("aggregateId", aggregateId);
            var documents = await collection.Find(filter).ToListAsync();

            var actualEventsCount = documents.Any() ? documents.Max(x => (UInt32)x.GetValue("aggregateVersion").AsInt64) : 0;
            if (expectedEventsCount != actualEventsCount)
            {
                _logger.LogError("Can't save events for aggregate {aggregateName} ({aggregateId}), it was already changed", aggregateMeta.Name, aggregateId);
                throw new Exception($"Can't save events for aggregate {aggregateMeta.Name} ({aggregateId}), it was already changed");
            }

            foreach (var @event in aggregateEvents)
            {
                expectedEventsCount++;
                var eventMeta = _domain.GetEventMeta(@event.GetType());
                var eventData = JsonSerializer.Serialize(@event, @event.GetType());
                var document = new BsonDocument
                {
                    { "aggregateId", aggregateId },
                    { "aggregateVersion", expectedEventsCount },
                    { "eventType", eventMeta.Name },
                    { "eventMoment", @event.EventOccurred },
                    { "eventData", eventData }
                };
                await collection.InsertOneAsync(document);
            }
        }

        public void Clear()
        {
            _mongo.DropDatabase("events");
        }

        private readonly DomainMeta _domain;
        private readonly IMongoClient _mongo;
        private readonly ILogger<MongoEventStore> _logger;
    }
}