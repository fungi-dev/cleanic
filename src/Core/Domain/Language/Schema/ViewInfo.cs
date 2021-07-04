namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;

    public class ViewInfo : MessageInfo
    {
        public IReadOnlyCollection<QueryInfo> Queries { get; internal set; }

        public ViewInfo(Type viewType) : base(viewType)
        {
            EnsureTermTypeCorrect(viewType, typeof(View));
        }
    }
}