namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public class ProjectionSchema
    {
        public LanguageSchema Language { get; internal set; }
        public IReadOnlyCollection<ProjectorInfo> Projectors { get; internal set; }

        public ProjectionSchema()
        {
            Projectors = Array.Empty<ProjectorInfo>().ToImmutableHashSet();
        }
    }
}