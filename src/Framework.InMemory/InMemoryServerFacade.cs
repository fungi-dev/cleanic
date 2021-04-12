namespace Cleanic.Application
{
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    public class InMemoryServerFacade : IServerFacade
    {
        public InMemoryServerFacade(LanguageSchema languageSchema, ICommandBus commandBus, IViewStore viewStore, ILogger<InMemoryServerFacade> logger)
        {
            _languageSchema = languageSchema ?? throw new ArgumentNullException(nameof(languageSchema));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _viewStore = viewStore ?? throw new ArgumentNullException(nameof(viewStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AggregateView> Get(Query query)
        {
            var queryInfo = _languageSchema.GetQuery(query.GetType());
            var aggregateViewInfo = _languageSchema.GetAggregateView(queryInfo);
            return await _viewStore.Load(aggregateViewInfo, query.AggregateId);
        }

        public async Task Do(Command command)
        {
            await _commandBus.Send(command);
        }

        public void ListenViewUpdates(AggregateViewInfo aggregateViewInfo, Func<AggregateView, Task> listener)
        {
            throw new NotImplementedException();
        }

        private readonly LanguageSchema _languageSchema;
        private readonly ICommandBus _commandBus;
        private readonly IViewStore _viewStore;
        private readonly ILogger _logger;
    }
}