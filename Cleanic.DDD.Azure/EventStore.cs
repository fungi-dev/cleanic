using Cleanic.Application;
using Cleanic.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Framework
{
    public class EventStore : IEventStore
    {
        public EventStore(String connectionString)
        {
            _serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                DateFormatString = "yyyy-MM-ddThh:mm:ssZ",
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("events");
            _table.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }

        public async Task<IEvent[]> Load(IIdentity aggregateId)
        {
            if (aggregateId == null) throw new ArgumentNullException(nameof(aggregateId));

            var descriptors = await ReadDescriptors(aggregateId.Value);
            var events = new List<IEvent>();
            foreach (var d in descriptors)
            {
                var joined = String.Join("", d.Value);
                events.Add(JsonConvert.DeserializeObject<IEvent>(joined, _serializerSettings));
            }

            return events.ToArray();
        }

        public async Task Save(IIdentity aggregateId, UInt32 lastVersion, IEnumerable<IEvent> newEvents)
        {
            var events = newEvents.ToArray();
            if (aggregateId == null) throw new ArgumentNullException(nameof(aggregateId));
            if (events == null || events.Length == 0) throw new ArgumentNullException(nameof(events));

            var descriptors = await ReadDescriptors(aggregateId.Value);
            if (descriptors.Any())
            {
                if (lastVersion != descriptors.Last().Key) throw new Exception();
            }

            var batch = new TableBatchOperation();
            foreach (var @event in events)
            {
                lastVersion++;
                var eventData = JsonConvert.SerializeObject(@event, _serializerSettings);
                var chunkLength = 31 * 1024;
                if (eventData.Length <= chunkLength)
                {
                    batch.Insert(new EventsTableEntity
                    {
                        PartitionKey = aggregateId.Value,
                        RowKey = lastVersion.ToString(),
                        Event = eventData
                    });
                }
                else
                {
                    var chunks = eventData.Length / chunkLength + 1;
                    for (var i = 1; i <= chunks; i++)
                    {
                        var l = chunkLength < eventData.Length ? chunkLength : eventData.Length;
                        var chunk = eventData.Substring(0, l);
                        eventData = eventData.Substring(l);
                        batch.Insert(new EventsTableEntity
                        {
                            PartitionKey = aggregateId.Value,
                            RowKey = $"{lastVersion}{_longEventChunksDelimiter}{i}",
                            Event = chunk
                        });
                    }
                }
            }

            try
            {
                _table.ExecuteBatchAsync(batch).GetAwaiter().GetResult();
            }
            catch (StorageException e)
            {
                throw new Exception(e.RequestInformation.ExtendedErrorInformation.ErrorMessage, e);
            }
        }

        public async Task Clear()
        {
            TableContinuationToken token = null;
            var entities = new List<DynamicTableEntity>();
            do
            {
                var queryResult = await _table.ExecuteQuerySegmentedAsync(new TableQuery<DynamicTableEntity>(), token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            foreach (var entity in entities)
            {
                var delete = TableOperation.Delete(entity);
                await _table.ExecuteAsync(delete);
            }
        }

        private readonly JsonSerializerSettings _serializerSettings;
        private readonly CloudTable _table;
        private readonly String _longEventChunksDelimiter = "-";

        private async Task<Dictionary<UInt32, List<String>>> ReadDescriptors(String partition)
        {
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partition);
            var query = new TableQuery<EventsTableEntity>().Where(filter);

            TableContinuationToken token = null;
            var descriptors = new Dictionary<UInt32, List<String>>();
            do
            {
                var results = await _table.ExecuteQuerySegmentedAsync(query, token);
                foreach (var item in results.Results)
                {
                    if (!item.RowKey.Contains(_longEventChunksDelimiter))
                    {
                        descriptors.Add(UInt32.Parse(item.RowKey), new List<String> { item.Event });
                    }
                    else
                    {
                        var version = UInt32.Parse(item.RowKey.Split(_longEventChunksDelimiter.ToCharArray())[0]);
                        if (!descriptors.TryGetValue(version, out var chunks))
                        {
                            descriptors.Add(version, chunks = new List<String>());
                        }
                        chunks.Add(item.Event);
                    }
                }
                token = results.ContinuationToken;
            } while (token != null);

            return descriptors;
        }
    }
}