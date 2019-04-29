using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cleanic.Domain;

namespace Cleanic.DomainInfo
{
    /// <summary>
    /// Information about particular projection type in domain.
    /// </summary>
    public class ProjectionInfo
    {
        /// <summary>
        /// Collect information about passed projection type.
        /// </summary>
        public ProjectionInfo(Type projectionType)
        {
            Type = projectionType ?? throw new ArgumentNullException(nameof(projectionType));
            var projectionTypeInfo = projectionType.GetTypeInfo();
            if (!typeof(Projection).GetTypeInfo().IsAssignableFrom(projectionTypeInfo))
            {
                throw new ArgumentException("Attempt to build projection model for non-projection type!");
            }

            _eventAppliers = (from m in projectionType.GetRuntimeMethods()
                              where !m.IsStatic
                              let p = m.GetParameters()
                              where p.Length == 1
                              let t = p[0].ParameterType
                              where typeof(Event).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())
                              group m by t
                              into gm
                              select gm).ToDictionary(x => x.Key, x => x.ToArray());

            //todo переделать в инстанс-метод, где будут перечислены все варианты. вызывать его будет инфрструктура на незагруженном из базы объекте
            _idExtractors = (from m in projectionType.GetRuntimeMethods()
                             let p = m.GetParameters()
                             where p.Length == 1
                             let t = p[0].ParameterType
                             where typeof(Event).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())
                             let r = m.ReturnType
                             where r == typeof(String)
                             where m.IsStatic
                             select new { T = t, M = m }).ToDictionary(x => x.T, x => x.M);

            InfluencingEventTypes = _eventAppliers.Keys.ToArray();
        }

        /// <summary>
        /// Underlying CLR type of projection class.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Types of events which lead to projection modification.
        /// </summary>
        public Type[] InfluencingEventTypes { get; }

        /// <summary>
        /// Get action which apply data from event instance to passed projection.
        /// </summary>
        public Action<Projection, Event> GetEventApplier(Type eventType)
        {
            if (eventType == null) throw new ArgumentNullException(nameof(eventType));
            if (!typeof(Event).GetTypeInfo().IsAssignableFrom(eventType.GetTypeInfo()))
            {
                throw new ArgumentException($"{eventType} is not an event type!");
            }

            return (p, e) =>
            {
                var t = e.GetType();
                var appliers = _eventAppliers.ContainsKey(t) ? _eventAppliers[t] : Array.Empty<MethodInfo>();
                foreach (var a in appliers) a.Invoke(p, new Object[] { e });
            };
        }

        /// <summary>
        /// Get action which extract projection identifier from passed event.
        /// </summary>
        public Func<Event, String> GetIdFromEventExtractor(Type eventType)
        {
            if (eventType == null) throw new ArgumentNullException(nameof(eventType));
            if (!typeof(Event).GetTypeInfo().IsAssignableFrom(eventType.GetTypeInfo()))
            {
                throw new ArgumentException($"{eventType} is not an event type!");
            }

            return e =>
            {
                var t = e.GetType();
                if (!_idExtractors.ContainsKey(t)) return e.AggregateId;
                return (String)_idExtractors[t].Invoke(null, new Object[] { e });
            };
        }

        private readonly Dictionary<Type, MethodInfo[]> _eventAppliers;
        private readonly Dictionary<Type, MethodInfo> _idExtractors;
    }
}