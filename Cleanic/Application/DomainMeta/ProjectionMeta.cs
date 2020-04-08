﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cleanic.Core
{
    /// <summary>
    /// Describes projection and provide ability to use it regardless concrete domain specifics.
    /// </summary>
    public class ProjectionMeta : DomainObjectMeta
    {
        public AggregateMeta Aggregate { get; }
        public IReadOnlyCollection<QueryMeta> Queries { get; internal set; }
        public IReadOnlyCollection<EventMeta> Events { get; internal set; }

        public ProjectionMeta(TypeInfo projectionType, TypeInfo builderType, AggregateMeta aggregateMeta) : base(projectionType)
        {
            Aggregate = aggregateMeta ?? throw new ArgumentNullException(nameof(aggregateMeta));
            _builderType = builderType;
        }

        public Boolean IsHandlingEvent(Type eventType)
        {
            return _builderType.DeclaredMethods
                .Where(m => !m.IsStatic)
                .Any(m => m.GetParameters().Any(p => p.ParameterType == eventType));
        }

        public void HandleEvent(Projection projection, Event @event)
        {
            var method = _builderType.DeclaredMethods
                .Where(x => !x.IsStatic)
                .Where(x => x.GetParameters().Length == 1)
                .Single(x => x.GetParameters()[0].ParameterType == @event.GetType());
            var builder = (ProjectionBuilder)Activator.CreateInstance(_builderType.AsType(), projection);
            method.Invoke(builder, new Object[] { @event });
        }

        public String GetIdFromEvent(Event @event)
        {
            var method = _builderType.DeclaredMethods
                .Where(x => x.IsStatic)
                .Where(x => x.GetParameters().Length == 1)
                .SingleOrDefault(x => typeof(Event).GetTypeInfo().IsAssignableFrom(x.GetParameters()[0].ParameterType));
            return (String)method?.Invoke(null, new[] { @event }) ?? @event.AggregateId;
        }

        private readonly TypeInfo _builderType;
    }
}