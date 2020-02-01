namespace Cleanic.Core
{
    public abstract class Command<T> : ValueObject, ICommand<T>
        where T : IEntity<T>
    {
        protected Command(IIdentity<T> entityId)
        {
            EntityId = entityId;
        }

        public IIdentity<T> EntityId { get; }

        IIdentity ICommand.EntityId => EntityId;
    }
}