using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cleanic.Application
{
    public class QueryMeta : DomainObjectMeta, IEquatable<QueryMeta>
    {
        public ProjectionMeta Projection { get; }

        public QueryMeta(TypeInfo queryType, ProjectionMeta projectionMeta) : base(queryType)
        {
            Projection = projectionMeta ?? throw new ArgumentNullException(nameof(projectionMeta));
        }

        public override Boolean Equals(Object obj) => Equals(obj as QueryMeta);

        public Boolean Equals(QueryMeta other)
        {
            return other != null && EqualityComparer<Type>.Default.Equals(Type, other.Type);
        }

        public override Int32 GetHashCode()
        {
            return 2049151605 + EqualityComparer<Type>.Default.GetHashCode(Type);
        }
    }
}