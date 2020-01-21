using System;

namespace Cleanic.Core
{
    public interface ICommand
    {
        IIdentity EntityId { get; }
        Type EntityType { get; }
    }

    public interface ICommand<T> : ICommand
        where T : IEntity<T>
    {
        new IIdentity<T> EntityId { get; }
    }

    public interface ICommandResult
    { }

    public interface ICommandResult<TCommand, TEntity> : ICommandResult
        where TCommand : ICommand<TEntity>
        where TEntity : IEntity<TEntity>
    { }
}