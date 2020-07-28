using System;

namespace Cleanic
{
    public class CommandInfo : AggregateItemInfo
    {
        public CommandInfo(Type commandType, AggregateInfo aggregate) : base(commandType, aggregate) { }
    }
}