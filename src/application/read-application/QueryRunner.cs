using Cleanic.Core;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public abstract class QueryRunner
    {
        public QueryRunner(IEventStore eventStore, IProjectionStore projectionStore, Type queryType, ProjectionsInfo projectionsInfo, LanguageInfo languageInfo)
        {
            EventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _projectionStore = projectionStore ?? throw new ArgumentNullException(nameof(projectionStore));
            if (queryType == null) throw new ArgumentNullException(nameof(queryType));
            ProjectionsInfo = projectionsInfo ?? throw new ArgumentNullException(nameof(projectionsInfo));
            LanguageInfo = languageInfo ?? throw new ArgumentNullException(nameof(languageInfo));
            Query = LanguageInfo.GetQuery(queryType);
        }

        public QueryInfo Query { get; }

        public async Task<QueryResult> Run(Query query)
        {
            var type = query.GetType();
            if (type != Query.Type) throw new ArgumentException($"Query for this runner is {Query.Name}, not {type.FullName}", nameof(query));

            var task = (Task)GetRunMethod().Invoke(this, new Object[] { query });
            await task;
            var taskResultProperty = typeof(Task<>).MakeGenericType(Query.ResultType).GetRuntimeProperty(nameof(Task<Type>.Result));
            return (QueryResult)taskResultProperty.GetValue(task);
        }

        protected readonly IEventStore EventStore;
        protected readonly ProjectionsInfo ProjectionsInfo;
        protected readonly LanguageInfo LanguageInfo;

        protected async Task<T> BuildProjection<T>(String id)
            where T : Projection, new()
        {
            var projection = new T { AggregateId = id };
            var projectionInfo = ProjectionsInfo.GetProjection(typeof(T));
            if (projectionInfo.Materialized)
            {
                projection = (T)await _projectionStore.Load(projectionInfo, id) ?? projection;
            }
            else
            {
                var events = await EventStore.LoadEvents(projectionInfo.Events);
                foreach (var @event in events)
                {
                    var idFromEvent = projectionInfo.GetIdFromEvent(@event);
                    if (!idFromEvent.Equals(projection.AggregateId)) continue;
                    projection.Apply(@event);
                }
            }
            return projection;
        }

        private readonly IProjectionStore _projectionStore;

        private MethodInfo GetRunMethod()
        {
            var method = GetType().GetTypeInfo().DeclaredMethods
                .Where(m => m.GetParameters().Length == 1)
                .Where(m => m.GetParameters().Any(p => p.ParameterType == Query.Type))
                .SingleOrDefault();
            if (method != null) return method;
            throw new Exception($"'{GetType().FullName}' don't know how to handle a '{Query.Name}'");
        }
    }
}