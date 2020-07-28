using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Cleanic
{
    public class LanguageInfoBuilder
    {
        public LanguageInfoBuilder Aggregate<T>()
        {
            AddLanguageTypesFromAssembly(typeof(T).GetTypeInfo().Assembly);

            var aggregateType = typeof(T);
            _aggregates.Add(aggregateType);
            _aggregateCommands.Add(aggregateType, new List<Type>());
            _aggregateEvents.Add(aggregateType, new List<Type>());
            _aggregateQueries.Add(aggregateType, new List<Type>());

            var nested = aggregateType.GetTypeInfo().DeclaredNestedTypes;

            var commandTypes = nested.Where(x => x.IsSubclassOf(typeof(Command)));
            _aggregateCommands[aggregateType].AddRange(commandTypes.Select(x => x.AsType()));

            var eventTypes = nested.Where(x => x.IsSubclassOf(typeof(Event)));
            _aggregateEvents[aggregateType].AddRange(eventTypes.Select(x => x.AsType()));

            var queryTypes = nested.Where(x => x.IsSubclassOf(typeof(Query)));
            _aggregateQueries[aggregateType].AddRange(queryTypes.Select(x => x.AsType()));

            return this;
        }

        public void Build(LanguageInfo languageInfo)
        {
            var aggregates = new List<AggregateInfo>();
            foreach (var agg in _aggregates)
            {
                var aggMeta = new AggregateInfo(agg);
                aggMeta.Commands = _aggregateCommands[agg].Select(x => new CommandInfo(x, aggMeta)).ToImmutableHashSet();
                aggMeta.Events = _aggregateEvents[agg].Select(x => new EventInfo(x, aggMeta)).ToImmutableHashSet();
                aggMeta.Queries = _aggregateQueries[agg].Select(x => new QueryInfo(x, aggMeta)).ToImmutableHashSet();
                aggregates.Add(aggMeta);
            }

            languageInfo.Aggregates = aggregates.ToImmutableHashSet();
        }

        private readonly HashSet<Type> _languageTypes = new HashSet<Type>();
        private readonly List<Type> _aggregates = new List<Type>();
        private readonly Dictionary<Type, List<Type>> _aggregateCommands = new Dictionary<Type, List<Type>>();
        private readonly Dictionary<Type, List<Type>> _aggregateEvents = new Dictionary<Type, List<Type>>();
        private readonly Dictionary<Type, List<Type>> _aggregateQueries = new Dictionary<Type, List<Type>>();

        private void AddLanguageTypesFromAssembly(Assembly assembly)
        {
            var types = assembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(Command)) || x.IsSubclassOf(typeof(Event)) || x.IsSubclassOf(typeof(Query)));
            foreach (var t in types) _languageTypes.Add(t.AsType());
        }
    }
}