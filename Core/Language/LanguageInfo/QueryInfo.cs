using Cleanic.Core;
using System;
using System.Linq;
using System.Reflection;

namespace Cleanic
{
    public class QueryInfo : AggregateItemInfo
    {
        public QueryInfo(Type queryType, AggregateInfo aggregate) : base(queryType, aggregate)
        {
            ResultType = Type.GetTypeInfo().DeclaredNestedTypes.Single(x => x.IsSubclassOf(typeof(QueryResult))).AsType();
        }

        public Type ResultType { get; }
    }
}