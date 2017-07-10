using System.Reflection;

namespace OpenDomainModel
{
    public class ProjectionInfo : MessageInfo
    {
        public override string DomainType => "Projection";

        public ProjectionInfo(TypeInfo type, string context = "") : base(type, context) { }
    }
}