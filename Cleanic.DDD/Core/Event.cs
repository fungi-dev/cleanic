namespace Cleanic.Core
{
    public abstract class Event<T> : ValueObject, IEvent<T>
        where T : IEntity<T>
    {
        protected Event(IIdentity<T> entityId)
        {
            EntityId = entityId;
        }

        public IIdentity<T> EntityId { get; }

        IIdentity IEvent.EntityId => EntityId;
    }

    public abstract class Error<T> : Event<T>, IError<T>
        where T : IEntity<T>
    {
        protected Error(IIdentity<T> entityId) : base(entityId) { }
    }
}