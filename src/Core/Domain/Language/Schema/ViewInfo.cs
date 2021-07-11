namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    public sealed class ViewInfo : MessageInfo
    {
        public static ViewInfo Get(Type type) => (ViewInfo)Get(type, () => new ViewInfo(type));

        private ViewInfo(Type viewType) : base(viewType)
        {
            EnsureTermTypeCorrect<View>(viewType);

            var queryTypes = Type.GetTypeInfo().DeclaredNestedTypes.Where(x => typeof(Query).IsAssignableFrom(x));
            Queries = queryTypes.Select(x => QueryInfo.Get(x)).ToImmutableHashSet();
        }

        public IReadOnlyCollection<QueryInfo> Queries { get; }
    }
}