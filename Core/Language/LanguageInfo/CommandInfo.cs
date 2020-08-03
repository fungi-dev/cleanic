using System;

namespace Cleanic
{
    public class CommandInfo : ActionInfo
    {
        public CommandInfo(Type commandType, AggregateInfo aggregate) : base(commandType, aggregate) { }
    }
}