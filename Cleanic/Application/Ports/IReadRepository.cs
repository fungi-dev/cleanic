using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IReadRepository
    {
        Task<IProjection> Load(Type type, IIdentity id);
        Task Save(IProjection projection);
    }
}