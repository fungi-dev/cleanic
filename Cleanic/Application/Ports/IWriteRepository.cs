using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IWriteRepository
    {
        Task<IEntity> LoadOrCreate(IIdentity id, Type type);
        Task<IEvent[]> Save(IEntity entity);
    }
}