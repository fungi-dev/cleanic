namespace Cleanic.Application
{
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class ViewRepository
    {
        public ViewRepository(IViewStore viewStore, ILogger<ViewRepository> logger)
        {
            _viewStore = viewStore ?? throw new ArgumentNullException(nameof(viewStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AggregateView> Load(AggregateViewInfo aggregateViewInfo, String aggregateId)
        {
            if (aggregateViewInfo == null) throw new ArgumentNullException(nameof(aggregateViewInfo));
            if (String.IsNullOrWhiteSpace(aggregateId)) throw new ArgumentNullException(nameof(aggregateId));

            var view = await _viewStore.Load(aggregateViewInfo, aggregateId);
            if (aggregateViewInfo.BelongsToRootAggregate && view == null)
            {
                view = (AggregateView)Activator.CreateInstance(aggregateViewInfo.Type);
                view.AggregateId = aggregateId;
            }
            return view;
        }

        public async Task<AggregateView> Load(AggregateViewInfo aggregateViewInfo, Expression<Func<AggregateView,Boolean>> selector)
        {
            if (aggregateViewInfo == null) throw new ArgumentNullException(nameof(aggregateViewInfo));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return await _viewStore.LoadOneByFilter(aggregateViewInfo, selector);
        }

        public Task Save(AggregateView aggregateView) => _viewStore.Save(aggregateView);

        private readonly IViewStore _viewStore;
        private readonly ILogger _logger;
    }
}