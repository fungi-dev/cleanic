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

        public Task<View> Load(ViewInfo viewInfo, String entityId)
        {
            if (viewInfo == null) throw new ArgumentNullException(nameof(viewInfo));
            if (String.IsNullOrWhiteSpace(entityId)) throw new ArgumentNullException(nameof(entityId));

            if (!_db.ContainsKey(viewInfo.Type)) return Task.FromResult<View>(null);

            return Task.FromResult(_db[viewInfo.Type].ContainsKey(entityId) ? _db[viewInfo.Type][entityId] : null);
        }

        public Task<View[]> Load(ViewInfo viewInfo, Expression<Func<View, Boolean>> filterExpression)
        {
            if (viewInfo == null) throw new ArgumentNullException(nameof(viewInfo));
            if (filterExpression == null) throw new ArgumentNullException(nameof(filterExpression));

            if (!_db.ContainsKey(viewInfo.Type)) return Task.FromResult(Array.Empty<View>());

            return Task.FromResult(_db[viewInfo.Type].Values.Where(filterExpression.Compile()).ToArray());
        }

        public Task Save(View view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));

            if (!_db.TryGetValue(view.GetType(), out var entities))
            {
                _db.Add(view.GetType(), entities = new Dictionary<String, View>());
            }

            if (!entities.ContainsKey(view.EntityId))
            {
                entities.Add(view.EntityId, view);
            }
            else
            {
                entities[view.EntityId] = view;
            }

            return Task.CompletedTask;
        }

        public Task Clear()
        {
            _db.Clear();
            return Task.CompletedTask;
        }

        private readonly Dictionary<Type, Dictionary<String, View>> _db = new Dictionary<Type, Dictionary<String, View>>();
        private readonly ILogger _logger;
    }
}