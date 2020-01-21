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

    public interface IIdentity
    { }

    public interface IIdentity<T> : IIdentity
        where T : IEntity<T>
    { }
}