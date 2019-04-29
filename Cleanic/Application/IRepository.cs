using Cleanic.Domain;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IRepository
    {
        Task<IEntity> Load(String id, Type type);

        Task<T> Load<T>(String id) where T : IEntity;

        Task Save(IEntity entity);
    }
}