namespace Cleanic.Core
{
    using System;
    using System.Reflection;

    public class CommandInfo : DomainObjectInfo
    {
        public CommandInfo(Type commandType, AggregateInfo aggregate) : base(commandType, aggregate)
        {
            if (!commandType.GetTypeInfo().IsSubclassOf(typeof(Command))) throw new ArgumentOutOfRangeException(nameof(commandType));
        }
    }
}