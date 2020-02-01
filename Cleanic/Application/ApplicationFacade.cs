using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    //todo do logging
    public interface IApplicationFacade
    {
        Task Do(ICommand command);

        /// <summary>
        /// Query the application for some data.
        /// </summary>
        Task<TProjection> Get<TEntity, TProjection>(IQuery<TEntity, TProjection> query)
            where TEntity : IEntity
            where TProjection : IProjection<TEntity>;
    }

    public class ApplicationFacade : IApplicationFacade
    {
        public ApplicationFacade(ICommandBus bus, IReadRepository db)
        {
            _bus = bus;
            _db = db;
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

        private readonly ICommandBus _bus;
        private readonly IReadRepository _db;
    }
}