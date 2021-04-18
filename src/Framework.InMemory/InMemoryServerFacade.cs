namespace Cleanic.Application
{
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    public class InMemoryServerFacade : IServerFacade
    {
        public LanguageSchema Language { get; }

        public InMemoryServerFacade(LanguageSchema languageSchema, ICommandBus commandBus, ViewRepository viewRepository, ILogger<InMemoryServerFacade> logger)
        {
            Language = languageSchema ?? throw new ArgumentNullException(nameof(languageSchema));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _viewRepository = viewRepository ?? throw new ArgumentNullException(nameof(viewRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AggregateView> Get(Query query)
        {
            var queryInfo = Language.GetQuery(query.GetType());
            var aggregateViewInfo = Language.GetAggregateView(queryInfo);
            return await _viewRepository.Load(aggregateViewInfo, query.AggregateId);
        }

        public async Task Do(Command command)
        {
            await _commandBus.Send(command);
        }

        public void ListenViewUpdates(AggregateViewInfo aggregateViewInfo, Func<AggregateView, Task> listener)
        {
            throw new NotImplementedException();
        }

        private readonly ICommandBus _commandBus;
        private readonly ViewRepository _viewRepository;
        private readonly ILogger _logger;
    }
}