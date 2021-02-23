using Cleanic.Application;
using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cleanic.Framework
{
    public class InMemoryProjectionStore : IProjectionStore
    {
        public Task<Projection> Load(ProjectionInfo projectionInfo, String aggregateId)
        {
            if (!_db.ContainsKey(projectionInfo.Type)) return Task.FromResult<Projection>(null);

            return Task.FromResult(_db[projectionInfo.Type].ContainsKey(aggregateId) ? _db[projectionInfo.Type][aggregateId] : null);
        }

        public Task Save(Projection projection)
        {
            if (!_db.TryGetValue(projection.GetType(), out var entities))
            {
                _db.Add(projection.GetType(), entities = new Dictionary<String, Projection>());
            }

            if (!entities.ContainsKey(projection.AggregateId))
            {
                entities.Add(projection.AggregateId, projection);
            }
            else
            {
                entities[projection.AggregateId] = projection;
            }

            return Task.CompletedTask;
        }

        public Task Clear()
        {
            _db.Clear();
            return Task.CompletedTask;
        }

        private readonly Dictionary<Type, Dictionary<String, Projection>> _db = new Dictionary<Type, Dictionary<String, Projection>>();
    }
}