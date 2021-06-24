namespace Cleanic.Core
{
    using System;

    public abstract class MessageInfo : DomainObjectInfo
    {
        public Boolean BelongsToRootAggregate { get; }

        protected MessageInfo(Type messageType, Boolean belongsToRootAggregate) : base(messageType)
        {
            BelongsToRootAggregate = belongsToRootAggregate;
        }
    }
}