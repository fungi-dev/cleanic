namespace Cleanic.Core
{
    public interface IEntity
    {
        IIdentity Id { get; }
    }

    public interface IEntity<T> : IEntity
        where T : IEntity<T>
    {
        new IIdentity<T> Id { get; }
    }
}