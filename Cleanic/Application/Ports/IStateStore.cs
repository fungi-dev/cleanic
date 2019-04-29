using Cleanic.Domain;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IStateStore
    {
        Task<Entity> Load(String entityId, Type entityType);
        Task Save(Entity entity);
        Task Clear();
    }
}