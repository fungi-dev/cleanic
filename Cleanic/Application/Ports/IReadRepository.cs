using Cleanic.Core;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IReadRepository
    {
        Task<TQueryResult> Load<TQueryResult>(IIdentity entityId)
            where TQueryResult : class, IQueryResult;
    }
}