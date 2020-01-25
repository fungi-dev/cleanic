namespace Cleanic.Core
{
    public abstract class QueryResult : ValueObject, IQueryResult
    {
        protected QueryResult(IIdentity entityId)
        {
            EntityId = entityId;
        }

        public IIdentity EntityId { get; }
    }

    public abstract class Query<T> : ValueObject, IQuery<T>
        where T : IQueryResult
    {
        protected Query(IIdentity entityId)
        {
            EntityId = entityId;
        }

        public IIdentity EntityId { get; }
    }
}