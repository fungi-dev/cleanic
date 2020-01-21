using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IWriteRepository
    {
        Task<IEntity> Load(IIdentity id, Type type);

        Task<IEvent[]> Save(IEntity entity);
    }
}