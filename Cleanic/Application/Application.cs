using Cleanic.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
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
            IRepository repository,
            DomainInfo.DomainInfo domain,
            Func<Type, IService[]> domainServiceFactory)
        {
            _bus = bus;
            Repository = repository;
            Domain = domain;
            new LogicAgent(bus, Repository, domain, domainServiceFactory);
            new ProjectionAgent(bus, Repository, domain);

            _commandResults = new List<Command.Result>();
            bus.ListenCommandResults(cr => SaveCommandResult(cr));
        }

        public DomainInfo.DomainInfo Domain { get; }
        protected IRepository Repository { get; }

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
        /// <param name="projectionId">Identifier of projection instance.</param>
        public async Task<T> Get<T>(String projectionId) where T : IProjection
        {
            var type = typeof(T);
            return (T)await Repository.Load(projectionId, type);
        }

        public Task<Command.Result> CheckCommand(String commandId)
        {
            var commandResult = _commandResults.FirstOrDefault(x => x.SubjectId == commandId);
            return Task.FromResult(commandResult);
        }

        private Task SaveCommandResult(Command.Result cr)
        {
            _commandResults.Add(cr);
            return Task.CompletedTask;
        }

        private readonly IMessageBus _bus;
        private readonly List<Command.Result> _commandResults;
    }
}