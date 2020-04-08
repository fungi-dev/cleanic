using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IProjectionStore
    {
        Task<Projection> Load(String id, Type type);
        Task Save(Projection projection);
        Task Clear();
    }
}