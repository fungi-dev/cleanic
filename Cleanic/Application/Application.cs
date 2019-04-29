using Cleanic.Domain;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    /// <summary>
    /// Facade for interact with the application.
    /// </summary>
    public abstract class Application
    {
        protected Application(
            IMessageBus bus,
            IEventStore events,
            IStateStore states,
            DomainInfo.DomainInfo domain,
            Func<Type, IDomainService[]> domainServiceFactory)
        {
            _bus = bus;
            Repository = new Repository(events, states);
            Domain = domain;
            new LogicAgent(bus, Repository, domain, domainServiceFactory);
            new ProjectionAgent(bus, Repository, domain);
        }

        public DomainInfo.DomainInfo Domain { get; }
        protected Repository Repository { get; }

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
            return (T)await Repository.Load(id, type);
        }

        private readonly IMessageBus _bus;
    }
}