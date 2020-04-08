using System;
using System.Collections.Generic;

namespace Cleanic.Core
{
    public abstract class ProjectionBuilder : DomainObject
    {
        protected override IEnumerable<Object> GetIdentityComponents() => Array.Empty<Object>();
    }

    public abstract class ProjectionBuilder<T> : ProjectionBuilder
        where T : Projection
    {
        public T Projection { get; }

        public ProjectionBuilder(T projection)
        {
            Projection = projection;
        }
    }
}