namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;

    public class MessageInfo : TermInfo, IEquatable<MessageInfo>
    {
        public AggregateInfo Aggregate { get; }

        public MessageInfo(Type messageType, AggregateInfo aggregate) : base(messageType)
        {
            Aggregate = aggregate ?? throw new ArgumentNullException(nameof(aggregate));
        }

        public override Boolean Equals(Object obj) => Equals(obj as MessageInfo);

        public Boolean Equals(MessageInfo other)
        {
            return other != null && EqualityComparer<Type>.Default.Equals(Type, other.Type);
        }

        public override Int32 GetHashCode()
        {
            return 2049151605 + EqualityComparer<Type>.Default.GetHashCode(Type);
        }

        public override String ToString() => Type.Name;
    }

    public class ActionInfo : MessageInfo
    {
        public ActionInfo(Type actionType, AggregateInfo aggregate) : base(actionType, aggregate) { }
    }
}