namespace Cleanic.Core
{
    using System;

    public sealed class CommandInfo : MessageInfo
    {
        public static CommandInfo Get(Type type) => (CommandInfo)Get(type, () => new CommandInfo(type));

        private CommandInfo(Type commandType) : base(commandType)
        {
            EnsureTermTypeCorrect<Command>(commandType);
        }
    }
}