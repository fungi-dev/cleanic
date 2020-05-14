using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class ReadApplicationFacade
    {
        public ReadApplicationFacade(Func<Type, QueryRunner> queryRunnerFactory)
        {
            _queryRunnerFactory = queryRunnerFactory ?? throw new ArgumentNullException(nameof(queryRunnerFactory));
        }

        public async Task<TQueryResult> Get<TQueryResult>(Query query)
            where TQueryResult : QueryResult
        {
            //todo serve multitanancy
            //todo do authentication
            //todo do authorization

            return (TQueryResult)await Get(query);
        }

        public async Task<QueryResult> Get(Query query)
        {
            var queryRunner = _queryRunnerFactory(query.GetType());
            return await queryRunner.Run(query);
        }

        private readonly Func<Type, QueryRunner> _queryRunnerFactory;
    }
}