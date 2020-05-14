using System;

namespace Cleanic.Application
{
    public class CommandInfo : AggregateItemInfo
    {
        public CommandInfo(Type commandType, AggregateInfo aggregate) : base(commandType, aggregate) { }
    }
}