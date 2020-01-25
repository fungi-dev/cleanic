using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface IApplicationFacade
    {
        Task Do(ICommand command);

        /// <summary>
        /// Query the application for some data.
        /// </summary>
        Task<TQueryResult> Get<TQueryResult>(IQuery<TQueryResult> query)
            where TQueryResult : class, IQueryResult;
    }

    public abstract class ApplicationFacade : IApplicationFacade
    {
        protected ApplicationFacade(ICommandBus bus, IReadRepository db)
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

        public async Task<TQueryResult> Get<TQueryResult>(IQuery<TQueryResult> query)
            where TQueryResult : class, IQueryResult
        {
            //todo serve multitanancy
            //todo do authentication
            //todo do authorization

            return await _db.Load<TQueryResult>(query.EntityId);
        }

        private readonly ICommandBus _bus;
        private readonly IReadRepository _db;
    }
}