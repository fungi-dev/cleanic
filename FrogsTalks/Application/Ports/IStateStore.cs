using FrogsTalks.Domain;
using System;
using System.Threading.Tasks;

namespace FrogsTalks.Application
{
    public interface IStateStore
    {
        Task<Entity> Load(String entityId, Type entityType);
        Task Save(Entity entity);
        Task Clear();
    }
}