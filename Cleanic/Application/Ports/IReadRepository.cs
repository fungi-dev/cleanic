using Cleanic.Core;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IReadRepository
    {
        Task<TQueryResult> Load<TQuery, TQueryResult>(IIdentity entityId)
            where TQuery : IQuery
            where TQueryResult : IQueryResult<TQuery>;
    }
}