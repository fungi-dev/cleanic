namespace Cleanic.Core
{
    using System;
    using System.Reflection;

    public class CommandInfo : DomainObjectInfo
    {
        public CommandInfo(Type commandType) : base(commandType)
        {
            if (!commandType.GetTypeInfo().IsSubclassOf(typeof(Command))) throw new ArgumentOutOfRangeException(nameof(commandType));
        }
    }
}