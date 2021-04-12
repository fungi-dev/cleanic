namespace Cleanic.Core
{
    using System;
    using System.Reflection;

    public class QueryInfo : DomainObjectInfo
    {
        public QueryInfo(Type queryType) : base(queryType)
        {
            if (!queryType.GetTypeInfo().IsSubclassOf(typeof(Query))) throw new ArgumentOutOfRangeException(nameof(queryType));
        }
    }
}