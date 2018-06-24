using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FrogsTalks.Application.Ports;
using FrogsTalks.Domain;

namespace FrogsTalks.Application
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
            }

            var agg = (Aggregate)entity;
            var newEvents = agg.FreshChanges.ToArray();
            var persistedVersion = agg.Version - newEvents.Length;
            await _events.Save(agg.Id, persistedVersion, newEvents);
        }

        #region Internals

        private readonly IEventStore _events;
        private readonly IStateStore _states;

        #endregion
    }
}