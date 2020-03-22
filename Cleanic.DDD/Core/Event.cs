using Newtonsoft.Json;
using System;

namespace Cleanic.Core
{
    public abstract class Event<T> : ValueObject, IEvent<T>
        where T : IEntity<T>
    {
        protected Event(IIdentity<T> entityId)
        {
            EntityId = entityId;
            Moment = DateTime.UtcNow;
        }

        [JsonProperty]
        public IIdentity<T> EntityId { get; private set; }

        [JsonProperty]
        public DateTime Moment { get; private set; }

        IIdentity IEvent.EntityId => EntityId;
    }

    public abstract class Error<T> : Event<T>, IError<T>
        where T : IEntity<T>
    {
        protected Error(IIdentity<T> entityId) : base(entityId) { }
    }
}