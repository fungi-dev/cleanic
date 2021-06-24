namespace Cleanic.Core
{
    using System;
    using System.Reflection;

    public class CommandInfo : MessageInfo
    {
        public CommandInfo(Type commandType, Boolean belongsToRootAggregate) : base(commandType, belongsToRootAggregate)
        {
            if (!commandType.GetTypeInfo().IsSubclassOf(typeof(Command))) throw new ArgumentOutOfRangeException(nameof(commandType));
        }
    }
}