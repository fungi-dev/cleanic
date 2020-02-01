using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class ReadRepository : IReadRepository
    {
        public ReadRepository(IStateStore store)
        {
            _store = store;
        }

        public async Task<IProjection> Load(Type type, IIdentity id)
        {
            var entity = await _store.Load(id, type);
            return entity as IProjection;
        }

        public async Task Save(IProjection projection)
        {
            await _store.Save(projection);
        }

        private readonly IStateStore _store;
    }
}