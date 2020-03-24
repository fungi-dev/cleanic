namespace Cleanic.Core
{
    public interface IProjection
    {
        IIdentity Id { get; set; }
    }

    public interface IProjection<T> : IProjection
        where T : IEntity
    {
        new IIdentity<T> Id { get; set; }
    }

    public interface IQuery
    {
        IIdentity Id { get; set; }
    }

    public interface IQuery<TEntity, TProjection> : IQuery
        where TEntity : IEntity
        where TProjection : IProjection<TEntity>
    {
        new IIdentity<TEntity> Id { get; set; }
    }
}