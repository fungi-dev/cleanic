namespace Cleanic.Framework
{
    using Cleanic.Application;
    using Cleanic.Core;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class InMemoryViewStore : IViewStore
    {
        public InMemoryViewStore(ILogger<InMemoryViewStore> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<AggregateView> Load(AggregateViewInfo aggregateViewInfo, String aggregateId)
        {
            if (aggregateViewInfo == null) throw new ArgumentNullException(nameof(aggregateViewInfo));
            if (String.IsNullOrWhiteSpace(aggregateId)) throw new ArgumentNullException(nameof(aggregateId));

            if (!_db.ContainsKey(aggregateViewInfo.Type)) return Task.FromResult<AggregateView>(null);

            return Task.FromResult(_db[aggregateViewInfo.Type].ContainsKey(aggregateId) ? _db[aggregateViewInfo.Type][aggregateId] : null);
        }

        public Task<AggregateView[]> Load(AggregateViewInfo aggregateViewInfo, Expression<Func<AggregateView, Boolean>> filterExpression)
        {
            if (aggregateViewInfo == null) throw new ArgumentNullException(nameof(aggregateViewInfo));
            if (filterExpression == null) throw new ArgumentNullException(nameof(filterExpression));

            if (!_db.ContainsKey(aggregateViewInfo.Type)) return Task.FromResult(Array.Empty<AggregateView>());

            return Task.FromResult(_db[aggregateViewInfo.Type].Values.Where(filterExpression.Compile()).ToArray());
        }

        public Task Save(AggregateView aggregateView)
        {
            if (aggregateView == null) throw new ArgumentNullException(nameof(aggregateView));

            if (!_db.TryGetValue(aggregateView.GetType(), out var entities))
            {
                _db.Add(aggregateView.GetType(), entities = new Dictionary<String, AggregateView>());
            }

            if (!entities.ContainsKey(aggregateView.AggregateId))
            {
                entities.Add(aggregateView.AggregateId, aggregateView);
            }
            else
            {
                entities[aggregateView.AggregateId] = aggregateView;
            }

            return Task.CompletedTask;
        }

        public Task Clear()
        {
            _db.Clear();
            return Task.CompletedTask;
        }

        private readonly Dictionary<Type, Dictionary<String, AggregateView>> _db = new Dictionary<Type, Dictionary<String, AggregateView>>();
        private readonly ILogger _logger;
    }
}