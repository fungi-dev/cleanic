﻿using Cleanic.Domain;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class Repository
    {
        public Repository(IEventStore events, IStateStore states)
        {
            _events = events;
            _states = states;
        }

        public async Task<Entity> Load(String id, Type type)
        {
            if (!type.GetTypeInfo().IsSubclassOf(typeof(Aggregate)))
            {
                return await _states.Load(id, type);
            }

            var agg = (Aggregate)Activator.CreateInstance(type, id);
            var persistedEvents = await _events.Load(id);
            agg.LoadFromHistory(persistedEvents);
            return agg;
        }

        public async Task<T> Load<T>(String id) where T : Entity
        {
            return (T)await Load(id, typeof(T));
        }

        public async Task Save(Entity entity)
        {
            if (!entity.GetType().GetTypeInfo().IsSubclassOf(typeof(Aggregate)))
            {
                await _states.Save(entity);
                return;
            }

            var agg = (Aggregate)entity;
            if (!agg.FreshChanges.Any()) throw new Exception("There is nothing to save!");
            var newEvents = agg.FreshChanges.ToArray();
            var persistedVersion = Convert.ToUInt32(agg.Version - newEvents.Length);
            await _events.Save(agg.Id, persistedVersion, newEvents);
        }

        #region Internals

        private readonly IEventStore _events;
        private readonly IStateStore _states;

        #endregion
    }
}