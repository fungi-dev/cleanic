using System;
using System.Linq;
using System.Reflection;
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

        public Entity Load(String id, Type type)
        {
            if (!type.GetTypeInfo().IsSubclassOf(typeof(Aggregate)))
            {
                return _states.Load(id, type);
            }

            var agg = (Aggregate)Activator.CreateInstance(type, id);
            var persistedEvents = _events.Load(id);
            agg.LoadFromHistory(persistedEvents);
            return agg;
        }

        public T Load<T>(String id) where T : Entity
        {
            return (T)Load(id, typeof(T));
        }

        public void Save(Entity entity)
        {
            if (!entity.GetType().GetTypeInfo().IsSubclassOf(typeof(Aggregate)))
            {
                _states.Save(entity);
                return;
            }

            var agg = (Aggregate)entity;
            var newEvents = agg.FreshChanges.ToArray();
            var persistedVersion = agg.Version - newEvents.Length;
            _events.Save(agg.Id, persistedVersion, newEvents);
        }

        #region Internals

        private readonly IEventStore _events;
        private readonly IStateStore _states;

        #endregion
    }
}