namespace Cleanic.Core
{
    public interface IQueryResult
    {
        IIdentity EntityId { get; }
    }

    public interface IQuery<T>
        where T : IQueryResult
    {
        IIdentity EntityId { get; }
    }
}