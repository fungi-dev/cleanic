namespace Cleanic.Application
{
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    public class InMemoryServerSdk : IServerSdk
    {
        public LanguageSchema Language { get; }

        public InMemoryServerSdk(LanguageSchema languageSchema, ICommandBus commandBus, IViewStore viewStore, ILogger<InMemoryServerSdk> logger)
        {
            Language = languageSchema ?? throw new ArgumentNullException(nameof(languageSchema));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _viewStore = viewStore ?? throw new ArgumentNullException(nameof(viewStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<View> Get(Query query, String accessToken)
        {
            if (String.IsNullOrWhiteSpace(accessToken)) throw new ArgumentNullException(nameof(accessToken));
            if (query == null) throw new ArgumentNullException(nameof(query));

            var queryInfo = Language.GetQuery(query.GetType());
            var viewInfo = Language.GetView(queryInfo);
            return await _viewStore.Load(viewInfo, query.EntityId);
        }

        public async Task Do(Command command, String accessToken)
        {
            if (String.IsNullOrWhiteSpace(accessToken)) throw new ArgumentNullException(nameof(accessToken));
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (command is IInternalCommand) throw new InvalidOperationException("Can't accept internal command from outside of application");

            await _commandBus.Send(command);
        }

        public void ListenViewUpdates(ViewInfo viewInfo, Func<View, Task> listener)
        {
            if (viewInfo == null) throw new ArgumentNullException(nameof(viewInfo));
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            throw new NotImplementedException();
        }

        private readonly ICommandBus _commandBus;
        private readonly IViewStore _viewStore;
        private readonly ILogger _logger;
    }
}