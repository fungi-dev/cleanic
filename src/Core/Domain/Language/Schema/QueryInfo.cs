namespace Cleanic.Core
{
    using System;

    public class QueryInfo : MessageInfo
    {
        public QueryInfo(Type queryType) : base(queryType)
        {
            EnsureTermTypeCorrect(queryType, typeof(Query));
        }
    }
}