namespace Cleanic.Application
{
    using Cleanic.Core;
    using System;
    using System.Collections.Generic;

    public class Client
    {
        public ApplicationState State { get; }
        public IReadOnlyCollection<CommandInfo> AvailableCommands => State.AvailableCommands;
        public CommandInfo RequestedCommand => State.RequestedCommand;
        public IReadOnlyCollection<QueryInfo> AvailableQueries => State.AvailableQueries;
        public AggregateView Data => State.Data;

        public Client(IServerFacade serverFacade)
        {
            _serverFacade = serverFacade;
        }

        public Client(IServerFacade serverFacade, String userName, String password) : this(serverFacade)
        {
            Login(userName, password);
        }

        public CommandResult Command(Command command) => throw new NotImplementedException();

        public void Query(Query query) => throw new NotImplementedException();

        public void Login(String userName, String password) => throw new NotImplementedException();

        private readonly IServerFacade _serverFacade;
    }
}