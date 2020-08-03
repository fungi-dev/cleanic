using System;
using System.Collections.Generic;
using System.Linq;

namespace Cleanic
{
    public class LanguageInfo
    {
        public IReadOnlyCollection<AggregateInfo> Aggregates { get; internal set; }

        public AggregateInfo GetAggregate(Type aggregateType)
        {
            var info = Aggregates.SingleOrDefault(x => x.Type == aggregateType);
            return info ?? throw new Exception($"No aggregate {aggregateType.FullName} in language");
        }

        public CommandInfo GetCommand(Type commandType)
        {
            var info = Aggregates.SelectMany(x => x.Commands).SingleOrDefault(x => x.Type == commandType);
            return info ?? throw new Exception($"No command {commandType.FullName} in language");
        }

        public EventInfo GetEvent(Type eventType)
        {
            var info = Aggregates.SelectMany(x => x.Events).SingleOrDefault(x => x.Type == eventType);
            return info ?? throw new Exception($"No event {eventType.FullName} in language");
        }

        public QueryInfo GetQuery(Type queryType)
        {
            var info = Aggregates.SelectMany(x => x.Queries).SingleOrDefault(x => x.Type == queryType);
            return info ?? throw new Exception($"No query {queryType.FullName} in language");
        }

        public Type FindCommand(String aggregateName, String commandName) => FindTerm(Aggregates.SelectMany(a => a.Commands), aggregateName, commandName);

        public Type FindEvent(String aggregateName, String eventName) => FindTerm(Aggregates.SelectMany(a => a.Events), aggregateName, eventName);

        public Type FindQuery(String aggregateName, String queryName) => FindTerm(Aggregates.SelectMany(a => a.Queries), aggregateName, queryName);

        public Type FindEvent(String eventFullName)
        {
            var e = eventFullName.ToLowerInvariant();
            var info = Aggregates.SelectMany(a => a.Events).SingleOrDefault(x => String.Equals(x.FullName, e, StringComparison.OrdinalIgnoreCase));
            return info?.Type ?? throw new Exception($"No event {eventFullName} in language");
        }

        private Type FindTerm(IEnumerable<MessageInfo> aggregateItems, String aggregateName, String aggregateItemName)
        {
            var a = aggregateName.ToLowerInvariant();
            var i = aggregateItemName.ToLowerInvariant();
            var aggregateItem = aggregateItems.SingleOrDefault(x => String.Equals(x.Aggregate.Name, a, StringComparison.OrdinalIgnoreCase) && String.Equals(x.Name, i, StringComparison.OrdinalIgnoreCase));
            return aggregateItem?.Type ?? throw new Exception($"No {aggregateItemName} in {aggregateName} aggregate");
        }
    }
}