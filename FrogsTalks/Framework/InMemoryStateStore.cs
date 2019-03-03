using FrogsTalks.Application;
using FrogsTalks.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrogsTalks.Framework
{
    public class InMemoryStateStore : IStateStore
    {
        public Task<Entity> Load(String entityId, Type entityType)
        {
            if (!_db.ContainsKey(entityType)) return Task.FromResult<Entity>(null);

            return Task.FromResult(_db[entityType].ContainsKey(entityId) ? _db[entityType][entityId] : null);
        }

        public Task Save(Entity entity)
        {
            if (!_db.TryGetValue(entity.GetType(), out var entities))
            {
                _db.Add(entity.GetType(), entities = new Dictionary<string, Entity>());
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

        private readonly Dictionary<Type, Dictionary<String, Entity>> _db = new Dictionary<Type, Dictionary<String, Entity>>();
    }
}