namespace Cleanic.Core
{
    using System;

    public sealed class QueryInfo : MessageInfo
    {
        public static QueryInfo Get(Type type) => (QueryInfo)Get(type, () => new QueryInfo(type));

        private QueryInfo(Type queryType) : base(queryType)
        {
            EnsureTermTypeCorrect<Query>(queryType);
        }
    }
}