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

    public interface IQuery<TEntity, TProjection>
        where TEntity : IEntity
        where TProjection : IProjection<TEntity>
    {
        IIdentity<TEntity> Id { get; set; }
    }
}