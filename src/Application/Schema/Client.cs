namespace Cleanic.Application
{
    using Cleanic.Core;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Client
    {
        public ApplicationState State { get; protected set; }
        public AggregateView View { get; protected set; }
        public IReadOnlyCollection<CommandInfo> AvailableCommands { get; protected set; }
        public CommandInfo RequestedCommand { get; protected set; }
        public IReadOnlyCollection<QueryInfo> AvailableQueries { get; protected set; }

        public Client(IServerFacade serverFacade)
        {
            ServerFacade = serverFacade;
        }

        public virtual Task Init() => Task.CompletedTask;

        public virtual Task Command(Command command) => ServerFacade.Do(command);

        public virtual async Task Query(Query query) => View = await ServerFacade.Get(query);

        protected readonly IServerFacade ServerFacade;
    }
}