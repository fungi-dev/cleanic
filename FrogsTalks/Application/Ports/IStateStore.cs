using System;
using System.Collections.Generic;
using FrogsTalks.Domain;

namespace FrogsTalks.Application.Ports
{
    public interface IStateStore
    {
        Entity Load(String id, Type entityType);
        void Save(Entity entity);
    }

    public class InMemoryStateStore : IStateStore
    {
        public Entity Load(String id, Type entityType)
        {
            return _db.ContainsKey(id) ? _db[id] : null;
        }

        public void Save(Entity entity)
        {
            if (!_db.ContainsKey(entity.Id))
            {
                _db.Add(entity.Id, entity);
            }
            else
            {
                _db[entity.Id] = entity;
            }
        }

        private readonly Dictionary<String, Entity> _db = new Dictionary<String, Entity>();
    }
}