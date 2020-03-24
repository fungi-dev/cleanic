﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cleanic.Core
{
    public class EventMeta : IEquatable<EventMeta>
    {
        public EventMeta(Type eventType, EntityMeta entityMeta)
        {
            Type = eventType ?? throw new ArgumentNullException(nameof(eventType));
            Name = eventType.Name;
            Entity = entityMeta ?? throw new ArgumentNullException(nameof(entityMeta));
        }

        public String Name { get; }
        public Type Type { get; }
        public EntityMeta Entity { get; }

        public override Boolean Equals(Object obj) => Equals(obj as EventMeta);

        public Boolean Equals(EventMeta other)
        {
            return other != null && EqualityComparer<Type>.Default.Equals(Type, other.Type);
        }

        public override Int32 GetHashCode()
        {
            return 2049151605 + EqualityComparer<Type>.Default.GetHashCode(Type);
        }

        public override String ToString() => Name;
    }

    public static class EventTypeExtensions
    {
        public static Boolean IsEvent(this Type type) => type.GetTypeInfo().IsEvent();
        public static Boolean IsEvent(this TypeInfo type) => type.AsType() == typeof(IEvent) || type.ImplementedInterfaces.Contains(typeof(IEvent));

        public static Boolean IsError(this Type type) => type.GetTypeInfo().IsError();
        public static Boolean IsError(this TypeInfo type) => type.AsType() == typeof(IError) || type.ImplementedInterfaces.Contains(typeof(IError));
        public static Boolean IsErrorCollection(this Type type) => type.GetTypeInfo().IsErrorCollection();
        public static Boolean IsErrorCollection(this TypeInfo type)
        {
            if (!type.ImplementedInterfaces.Contains(typeof(IEnumerable))) return false;
            if (!type.IsGenericType) return false;
            return type.GenericTypeArguments[0].IsError();
        }
    }
}