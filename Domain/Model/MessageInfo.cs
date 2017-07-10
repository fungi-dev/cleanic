using System.Linq;
using System.Reflection;

namespace OpenDomainModel
{
    public abstract class MessageInfo : DomainObjectInfo
    {
        public bool Anonymous { get; }

        public MessageInfo(TypeInfo type, string context = "") : base(type, context)
        {
            Anonymous = type.CustomAttributes.Any(x => x.AttributeType == typeof(AnonymousAttribute));
        }
    }
}