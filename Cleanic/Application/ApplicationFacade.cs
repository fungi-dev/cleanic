using Cleanic.Core;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    //todo do logging
    public class ApplicationFacade
    {
        public DomainMeta Domain { get; }

        public ApplicationFacade(ICommandBus bus, IReadRepository db, DomainMeta domain)
        {
            _bus = bus;
            _db = db;
            Domain = domain;
        }

        public async Task Do(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            //todo serve multitanancy
            //todo do authentication
            //todo do authorization
            //todo inject tech info to command (envelope)

            await _bus.Send(command);
        }

        public async Task<TProjection> Get<TEntity, TProjection>(Query query)
            where TEntity : Entity
            where TProjection : Projection
        {
            //todo serve multitanancy
            //todo do authentication
            //todo do authorization

            return (TProjection)await _db.Load(typeof(TProjection), query.AggregateId);
        }

        public async Task<Projection> Get(Query query)
        {
            var prjType = query.GetType().GetTypeInfo().DeclaringType;
            return await _db.Load(prjType, query.AggregateId);
        }

        private readonly ICommandBus _bus;
        private readonly IReadRepository _db;
    }
}