namespace Cleanic.Core
{
    using System;
    using System.Reflection;

    public class QueryInfo : MessageInfo
    {
        public QueryInfo(Type queryType, Boolean belongsToRootAggregate) : base(queryType, belongsToRootAggregate)
        {
            if (!queryType.GetTypeInfo().IsSubclassOf(typeof(Query))) throw new ArgumentOutOfRangeException(nameof(queryType));
        }
    }
}