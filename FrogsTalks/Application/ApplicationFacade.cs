using System;
using FrogsTalks.Application.Ports;
using FrogsTalks.Domain;
using FrogsTalks.UseCases;

namespace FrogsTalks.Application
{
    /// <summary>
    /// Facade for interact with the application.
    /// </summary>
    public abstract class ApplicationFacade
    {
        /// <summary>
        /// Create an instance of the application facade.
        /// </summary>
        /// <param name="bus">Bus where user commands will be sent.</param>
        /// <param name="db">Storage where queried data will be taken.</param>
        protected ApplicationFacade(IMessageBus bus, IProjectionsReader db)
        {
            _bus = bus;
            _db = db;
        }

        /// <summary>
        /// Order the application to do something.
        /// </summary>
        /// <param name="command">Command details.</param>
        public void Do(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            _bus.Send(command);
        }

        /// <summary>
        /// Query the application for some projection of the domain data.
        /// </summary>
        /// <typeparam name="T">Projection type.</typeparam>
        /// <param name="id">Identifier of projection instance.</param>
        /// <returns></returns>
        public T Get<T>(Guid id) where T : IProjection
        {
            return (T)_db.Load(id);
        }

        private readonly IMessageBus _bus;
        private readonly IProjectionsReader _db;
    }
}