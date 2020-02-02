using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cleanic.Core
{
    public interface IProjectionMeta
    {
        Type Type { get; }
        IReadOnlyCollection<EventMeta> Events { get; }

        IIdentity GetProjectionIdFromAffectingEvent(IEvent @event);
    }

    public static class ProjectionTypeExtensions
    {
        public static Boolean IsProjection(this Type type) => type.GetTypeInfo().IsProjection();
        public static Boolean IsProjection(this TypeInfo type) => type.ImplementedInterfaces.Contains(typeof(IProjection));
    }
}