using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IStateStore
    {
        Task<IEntity> Load(IIdentity entityId, Type entityType);
        Task Save(IEntity entity);
        Task Clear();
    }
}