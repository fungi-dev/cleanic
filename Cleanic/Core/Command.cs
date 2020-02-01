namespace Cleanic.Core
{
    public interface ICommand
    {
        IIdentity EntityId { get; }
    }

    public interface ICommand<T> : ICommand
        where T : IEntity<T>
    {
        new IIdentity<T> EntityId { get; }
    }
}