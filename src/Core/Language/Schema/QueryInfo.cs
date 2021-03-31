namespace Cleanic.Core
{
    using System;
    using System.Reflection;

    public class QueryInfo : ActionInfo
    {
        public AggregateViewInfo AggregateView { get; }

        public QueryInfo(Type queryType, AggregateViewInfo aggregateView) : base(queryType, aggregateView.Aggregate)
        {
            if (!queryType.GetTypeInfo().IsSubclassOf(typeof(Query))) throw new ArgumentOutOfRangeException(nameof(queryType));

            AggregateView = aggregateView ?? throw new ArgumentNullException(nameof(aggregateView));
        }
    }
}