using System;

namespace Cleanic.Core
{
    public interface IIdentity
    {
        String Value { get; }
    }

    public interface IIdentity<T> : IIdentity
        where T : IEntity
    { }
}