using System.Linq;
using System.Reflection;

namespace FrogsTalks
{
    public abstract class MessageInfo : DomainObjectInfo
    {
        public MessageInfo(TypeInfo type, string context = "") : base(type, context) { }
    }
}