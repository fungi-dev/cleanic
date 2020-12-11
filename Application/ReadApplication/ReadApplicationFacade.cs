using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class ReadApplicationFacade
    {
        public ReadApplicationFacade(IEventStore eventStore, IProjectionStore projectionStore, Func<Type, QueryRunner> queryRunnerFactory, LanguageInfo languageInfo)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _projectionStore = projectionStore ?? throw new ArgumentNullException(nameof(projectionStore));
            _queryRunnerFactory = queryRunnerFactory ?? throw new ArgumentNullException(nameof(queryRunnerFactory));
            _languageInfo = languageInfo ?? throw new ArgumentNullException(nameof(languageInfo));
        }

        public async Task<TQueryResult> Get<TQueryResult>(Query query)
            where TQueryResult : QueryResult
        {
            return (TQueryResult)await Get(query);
        }

        public async Task<QueryResult> Get(Query query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var queryInfo = _languageInfo.GetQuery(query.GetType());
            //todo check if this user allowed to read this aggregate
            // let admins set permissions for particular aggregates (and maybe for particular aggregate actions)

            var queryRunner = _queryRunnerFactory(query.GetType());
            return await queryRunner.Run(query);
        }

        public async Task RebuildProjections(ProjectionInfo projectionInfo)
        {
            var projections = new List<Projection>();

            var events = await _eventStore.LoadEvents(projectionInfo.Events);
            foreach (var e in events)
            {
                var idFromEvent = projectionInfo.GetIdFromEvent(e);
                var projection = projections.SingleOrDefault(x => x.AggregateId == idFromEvent);
                if (projection == null)
                {
                    projection = (Projection)Activator.CreateInstance(projectionInfo.Type);
                    projection.AggregateId = idFromEvent;
                    projections.Add(projection);
                }
                projection.Apply(e);
            }

            foreach (var projection in projections) await _projectionStore.Save(projection);
        }

        private readonly IEventStore _eventStore;
        private readonly IProjectionStore _projectionStore;
        private readonly Func<Type, QueryRunner> _queryRunnerFactory;
        private readonly LanguageInfo _languageInfo;
    }
}