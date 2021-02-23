using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IProjectionStore
    {
        Task<Projection> Load(ProjectionInfo projectionInfo, String aggregateId);
        Task Save(Projection projection);
    }
}