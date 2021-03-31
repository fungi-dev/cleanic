namespace Cleanic.Application
{
    using Cleanic.Core;
    using System.Collections.Generic;

    public class ApplicationState
    {
        public IReadOnlyCollection<CommandInfo> AvailableCommands { get; }
        public CommandInfo RequestedCommand { get; }
        public IReadOnlyCollection<QueryInfo> AvailableQueries { get; }
        public AggregateView Data { get; }
    }
}