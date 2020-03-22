using Cleanic.Application;
using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cleanic.Framework
{
    public class InMemoryStateStore : IProjectionStore
    {
        public Task<IProjection> Load(IIdentity id, Type type)
        {
            if (!_db.ContainsKey(type)) return Task.FromResult<IProjection>(null);

            return Task.FromResult(_db[type].ContainsKey(id) ? _db[type][id] : null);
        }

        public Task Save(IProjection projection)
        {
            if (!_db.TryGetValue(projection.GetType(), out var entities))
            {
                _db.Add(projection.GetType(), entities = new Dictionary<IIdentity, IProjection>());
            }

            if (!entities.ContainsKey(projection.Id))
            {
                entities.Add(projection.Id, projection);
            }
            else
            {
                entities[projection.Id] = projection;
            }

            return Task.CompletedTask;
        }

        public Task Clear()
        {
            _db.Clear();
            return Task.CompletedTask;
        }

        private readonly Dictionary<Type, Dictionary<IIdentity, IProjection>> _db = new Dictionary<Type, Dictionary<IIdentity, IProjection>>();
    }
}