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
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (String.IsNullOrWhiteSpace(accessToken)) throw new ArgumentNullException(nameof(accessToken));

            var queryInfo = Language.GetQuery(query.GetType());
            var viewInfo = Language.GetView(queryInfo);
            return await _viewStore.Load(viewInfo, query.EntityId);
        }

        public async Task Do(Command command, String accessToken)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (String.IsNullOrWhiteSpace(accessToken)) throw new ArgumentNullException(nameof(accessToken));

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