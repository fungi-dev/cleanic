namespace Cleanic.Application
{
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    public class InMemoryServerSdk : IServerSdk
    {
        public LanguageSchema Language { get; }

        public InMemoryServerSdk(LanguageSchema languageSchema, ICommandBus commandBus, ViewRepository viewRepository, ILogger<InMemoryServerSdk> logger)
        {
            Language = languageSchema ?? throw new ArgumentNullException(nameof(languageSchema));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _viewRepository = viewRepository ?? throw new ArgumentNullException(nameof(viewRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AggregateView> Get(Query query, String accessToken)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (String.IsNullOrWhiteSpace(accessToken)) throw new ArgumentNullException(nameof(accessToken));

            var queryInfo = Language.GetQuery(query.GetType());
            var aggregateViewInfo = Language.GetAggregateView(queryInfo);
            return await _viewRepository.Load(aggregateViewInfo, query.AggregateId);
        }

        public async Task Do(Command command, String accessToken)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (String.IsNullOrWhiteSpace(accessToken)) throw new ArgumentNullException(nameof(accessToken));

            await _commandBus.Send(command);
        }

        public void ListenViewUpdates(AggregateViewInfo aggregateViewInfo, Func<AggregateView, Task> listener)
        {
            if (aggregateViewInfo == null) throw new ArgumentNullException(nameof(aggregateViewInfo));
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            throw new NotImplementedException();
        }

        private readonly ICommandBus _commandBus;
        private readonly ViewRepository _viewRepository;
        private readonly ILogger _logger;
    }
}