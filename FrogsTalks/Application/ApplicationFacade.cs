using System;
using System.Threading.Tasks;
using FrogsTalks.Application.Ports;
using FrogsTalks.Domain;

namespace FrogsTalks.Application
{
    /// <summary>
    /// Facade for interact with the application.
    /// </summary>
    public class ApplicationFacade
    {
        /// <summary>
        /// Create an instance of the application facade.
        /// </summary>
        /// <param name="bus">Bus where user commands will be sent.</param>
        /// <param name="db">Storage where queried data will be taken.</param>
        public ApplicationFacade(IMessageBus bus, Repository db)
        {
            _bus = bus;
            _db = db;
        }

        /// <summary>
        /// Order the application to do something.
        /// </summary>
        /// <param name="command">Command details.</param>
        public async Task Do(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            await _bus.Send(command);
        }

        /// <summary>
        /// Query the application for some projection of the domain data.
        /// </summary>
        /// <typeparam name="T">Projection type.</typeparam>
        /// <param name="id">Identifier of projection instance.</param>
        public async Task<T> Get<T>(String id) where T : Projection
        {
            var type = typeof(T);
            return (T)await _db.Load(id, type);
        }

        private readonly IMessageBus _bus;
        private readonly Repository _db;
    }
}