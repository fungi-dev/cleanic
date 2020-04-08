using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IWriteRepository
    {
        Task<Aggregate> LoadOrCreate(String id, Type type);
        Task<Event[]> Save(Aggregate aggregate);
    }
}