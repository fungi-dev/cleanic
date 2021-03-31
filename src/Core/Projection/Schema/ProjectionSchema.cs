namespace Cleanic.Core
{
    using System.Collections.Generic;

    public class ProjectionSchema
    {
        public LanguageSchema Language { get; internal set; }
        public IReadOnlyCollection<ProjectorInfo> Projectors { get; internal set; }
    }
}