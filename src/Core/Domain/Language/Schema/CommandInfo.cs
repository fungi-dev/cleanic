namespace Cleanic.Core
{
    using System;

    public class CommandInfo : MessageInfo
    {
        public CommandInfo(Type commandType) : base(commandType)
        {
            EnsureTermTypeCorrect(commandType, typeof(Command));
        }
    }
}