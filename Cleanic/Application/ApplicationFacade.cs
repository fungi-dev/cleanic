using Cleanic.Core;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    //todo do logging
    public interface IApplicationFacade
    {
        IDomainFacade Domain { get; }

        Task Do(ICommand command);

        Task<TProjection> Get<TEntity, TProjection>(IQuery<TEntity, TProjection> query)
            where TEntity : IEntity
            where TProjection : IProjection<TEntity>;

        Task<IProjection> Get(IQuery query);
    }

    public class ApplicationFacade : IApplicationFacade
    {
        public IDomainFacade Domain { get; }

        public ApplicationFacade(ICommandBus bus, IReadRepository db, IDomainFacade domain)
        {
            _bus = bus;
            _db = db;
            Domain = domain;
        }

        public async Task Do(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            //todo serve multitanancy
            //todo do authentication
            //todo do authorization
            //todo inject tech info to command (envelope)

            await _bus.Send(command);
        }

        public async Task<TProjection> Get<TEntity, TProjection>(IQuery<TEntity, TProjection> query)
            where TEntity : IEntity
            where TProjection : IProjection<TEntity>
        {
            //todo serve multitanancy
            //todo do authentication
            //todo do authorization

            return (TProjection)await _db.Load(typeof(TProjection), query.Id);
        }

        public async Task<IProjection> Get(IQuery query)
        {
            var prjType = query.GetType().GetTypeInfo().DeclaringType;
            return await _db.Load(prjType, query.Id);
        }

        private readonly ICommandBus _bus;
        private readonly IReadRepository _db;
    }
}