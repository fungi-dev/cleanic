using Cleanic.Application;
using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cleanic.Framework
{
    public class InMemoryStateStore : IStateStore
    {
        public Task<IEntity> Load(IIdentity entityId, Type entityType)
        {
            if (!_db.ContainsKey(entityType)) return Task.FromResult<IEntity>(null);

            return Task.FromResult(_db[entityType].ContainsKey(entityId) ? _db[entityType][entityId] : null);
        }

        public Task Save(IEntity entity)
        {
            if (!_db.TryGetValue(entity.GetType(), out var entities))
            {
                _db.Add(entity.GetType(), entities = new Dictionary<IIdentity, IEntity>());
            }

            if (!entities.ContainsKey(entity.Id))
            {
                entities.Add(entity.Id, entity);
            }
            else
            {
                entities[entity.Id] = entity;
            }

            return Task.CompletedTask;
        }

        public Task Clear()
        {
            _db.Clear();
            return Task.CompletedTask;
        }

        private readonly Dictionary<Type, Dictionary<IIdentity, IEntity>> _db = new Dictionary<Type, Dictionary<IIdentity, IEntity>>();
    }
}