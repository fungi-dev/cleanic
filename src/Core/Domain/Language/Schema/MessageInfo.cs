namespace Cleanic.Core
{
    using System;

    public class MessageInfo : DomainObjectInfo
    {
        public MessageInfo(Type messageType) : base(messageType)
        {
            EnsureTermTypeCorrect(messageType, typeof(Message));
        }
    }
}