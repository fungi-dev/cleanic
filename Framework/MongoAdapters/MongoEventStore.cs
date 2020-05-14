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
        public MongoEventStore(String connectionString, ILogger<MongoEventStore> logger, LanguageInfo languageInfo)
        {
            if (String.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _mongo = new MongoClient(connectionString);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _languageInfo = languageInfo ?? throw new ArgumentNullException(nameof(languageInfo));
            _bus = new InMemoryEventBus(_logger);
            Db = _mongo.GetDatabase("events");
        }

        public IMongoDatabase Db { get; private set; }

        public async Task<Event[]> LoadEvents(AggregateInfo aggregateInfo, String aggregateId)
        {
            if (aggregateInfo == null) throw new ArgumentNullException(nameof(aggregateInfo));
            if (String.IsNullOrWhiteSpace(aggregateId)) throw new ArgumentNullException(nameof(aggregateId));

            var collection = Db.GetCollection<BsonDocument>(aggregateInfo.FullName);
            var filter = new BsonDocument("aggregateId", aggregateId);
            var documents = await collection.Find(filter).ToListAsync();

            var events = new List<Event>();
            foreach (var doc in documents)
            {
                var eventTypeName = doc.GetValue("eventType").AsString;
                var eventType = _languageInfo.FindEvent(eventTypeName);
                var eventData = doc.GetValue("eventData").AsString;
                events.Add((Event)JsonSerializer.Deserialize(eventData, eventType));
            }
            return events.ToArray();
        }

        public async Task<Event[]> LoadEvents(IEnumerable<EventInfo> eventInfos)
        {
            eventInfos = eventInfos?.ToArray();
            if (eventInfos == null || !eventInfos.Any()) throw new ArgumentNullException(nameof(eventInfos));

            var events = new List<Event>();
            foreach (var aggGroup in eventInfos.GroupBy(x => x.Aggregate.FullName))
            {
                var collection = Db.GetCollection<BsonDocument>(aggGroup.Key);
                foreach (var eventTypeGroup in aggGroup.GroupBy(x => x.FullName))
                {
                    var filter = new BsonDocument("eventType", eventTypeGroup.Key);
                    var documents = await collection.Find(filter).ToListAsync();
                    foreach (var doc in documents)
                    {
                        var eventTypeName = doc.GetValue("eventType").AsString;
                        var eventType = _languageInfo.FindEvent(eventTypeName);
                        var eventData = doc.GetValue("eventData").AsString;
                        events.Add((Event)JsonSerializer.Deserialize(eventData, eventType));
                    }
                }
            }

            return events.OrderBy(x => x.EventOccurred).ToArray();
        }

        public async Task SaveEvents(String aggregateId, UInt32 expectedEventsCount, IEnumerable<Event> events)
        {
            if (aggregateId == null) throw new ArgumentNullException(nameof(aggregateId));
            events = events?.ToArray();
            if (events == null || !events.Any()) throw new ArgumentNullException(nameof(events));

            var aggregateInfo = events.Select(x => _languageInfo.GetEvent(x.GetType()).Aggregate).Distinct().Single();

            var collection = Db.GetCollection<BsonDocument>(aggregateInfo.FullName);
            var filter = new BsonDocument("aggregateId", aggregateId);
            var documents = await collection.Find(filter).ToListAsync();

            var actualEventsCount = documents.Any() ? documents.Max(x => (UInt32)x.GetValue("aggregateVersion").AsInt64) : 0;
            if (expectedEventsCount != actualEventsCount)
            {
                _logger.LogError("Can't save events for aggregate {aggregateName} ({aggregateId}), it was already changed", aggregateInfo.Name, aggregateId);
                throw new Exception($"Can't save events for aggregate {aggregateInfo.Name} ({aggregateId}), it was already changed");
            }

            foreach (var @event in events)
            {
                expectedEventsCount++;
                var eventMeta = _languageInfo.GetEvent(@event.GetType());
                var eventData = JsonSerializer.Serialize(@event, @event.GetType());
                var document = new BsonDocument
                {
                    { "aggregateId", aggregateId },
                    { "aggregateVersion", expectedEventsCount },
                    { "eventType", eventMeta.FullName },
                    { "eventMoment", @event.EventOccurred },
                    { "eventData", eventData }
                };
                await collection.InsertOneAsync(document);
                await _bus.Publish(@event);
            }
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

        private readonly LanguageInfo _languageInfo;
        private readonly IMongoClient _mongo;
        private readonly ILogger<MongoEventStore> _logger;
        private readonly InMemoryEventBus _bus;
    }
}