using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrogsTalks.Domain;

namespace FrogsTalks.Application.Ports
{
    public interface IStateStore
    {
        Task<Entity> Load(String id, Type entityType);
        Task Save(Entity entity);
    }

    public class InMemoryStateStore : IStateStore
    {
        public Task<Entity> Load(String id, Type entityType)
        {
            return Task.FromResult(_db.ContainsKey(id) ? _db[id] : null);
        }

        public Task Save(Entity entity)
        {
            if (!_db.ContainsKey(entity.Id))
            {
                _db.Add(entity.Id, entity);
            }
            else
            {
                _db[entity.Id] = entity;
            }

            return Task.CompletedTask;
        }

        private readonly Dictionary<String, Entity> _db = new Dictionary<String, Entity>();
    }
}