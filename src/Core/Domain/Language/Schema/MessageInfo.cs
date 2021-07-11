namespace Cleanic.Core
{
    using System;

    public abstract class MessageInfo : DomainObjectInfo
    {
        protected MessageInfo(Type messageType) : base(messageType) { }
    }
}