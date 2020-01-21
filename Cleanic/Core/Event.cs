using System;

namespace Cleanic.Core
{
    public interface IEvent
    {
        IIdentity EntityId { get; }
        DateTime Moment { get; }
    }

    public interface IEvent<T> : IEvent
        where T : IEntity<T>
    {
        new IIdentity<T> EntityId { get; }

        IEvent<T> HappenedWith(IIdentity<T> id, DateTime moment);
    }
}