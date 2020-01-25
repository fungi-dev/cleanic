using System;

namespace Cleanic.Core
{
    public abstract class Command<T> : ValueObject, ICommand<T>
        where T : Entity<T>
    {
        protected Command(IIdentity<T> entityId)
        {
            EntityId = entityId;
            EntityType = typeof(T);
        }

        public IIdentity<T> EntityId { get; }

        public Type EntityType { get; }

        IIdentity ICommand.EntityId => EntityId;
    }
}