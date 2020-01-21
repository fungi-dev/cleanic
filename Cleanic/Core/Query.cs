namespace Cleanic.Core
{
    public interface IQuery
    {
        IIdentity EntityId { get; }
    }

    public interface IQuery<T> : IQuery
        where T : IEntity<T>
    {
        new IIdentity<T> EntityId { get; }
    }

    public interface IQueryResult<TQuery>
        where TQuery : IQuery
    { }

    public interface IQueryResult<TQuery, TEntity> : IQueryResult<TQuery>
        where TQuery : IQuery<TEntity>
        where TEntity : IEntity<TEntity>
    { }
}