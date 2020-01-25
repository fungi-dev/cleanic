using Cleanic.Core;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class ReadRepository : IReadRepository
    {
        public ReadRepository(IStateStore store)
        {
            _store = store;
        }

        public async Task<TQueryResult> Load<TQueryResult>(IIdentity entityId)
            where TQueryResult : class, IQueryResult
        {
            var type = typeof(TQueryResult);
            var entity = await _store.Load(entityId, type);
            return entity as TQueryResult;
        }

        private readonly IStateStore _store;
    }
}