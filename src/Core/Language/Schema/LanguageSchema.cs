namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class LanguageSchema
    {
        public IReadOnlyCollection<AggregateInfo> Aggregates { get; internal set; }

        public AggregateInfo GetAggregate(Type aggregateType)
        {
            if (aggregateType == null) throw new ArgumentNullException(nameof(aggregateType));
            if (!aggregateType.GetTypeInfo().IsSubclassOf(typeof(Aggregate))) throw new ArgumentOutOfRangeException(nameof(aggregateType));

            var info = Aggregates.SingleOrDefault(x => x.Type == aggregateType);
            return info ?? throw new LanguageSchemaException($"No aggregate {aggregateType.FullName} in language");
        }

        public CommandInfo GetCommand(Type commandType)
        {
            if (commandType == null) throw new ArgumentNullException(nameof(commandType));
            if (!commandType.GetTypeInfo().IsSubclassOf(typeof(Command))) throw new ArgumentOutOfRangeException(nameof(commandType));

            var info = Aggregates.SelectMany(x => x.Commands).SingleOrDefault(x => x.Type == commandType);
            return info ?? throw new LanguageSchemaException($"No command {commandType.FullName} in language");
        }

        public DomainEventInfo GetDomainEvent(Type domainEventType)
        {
            if (domainEventType == null) throw new ArgumentNullException(nameof(domainEventType));
            if (!domainEventType.GetTypeInfo().IsSubclassOf(typeof(DomainEvent))) throw new ArgumentOutOfRangeException(nameof(domainEventType));

            var info = Aggregates.SelectMany(x => x.DomainEvents).SingleOrDefault(x => x.Type == domainEventType);
            return info ?? throw new LanguageSchemaException($"No domain event {domainEventType.FullName} in language");
        }

        public QueryInfo GetQuery(Type queryType)
        {
            if (queryType == null) throw new ArgumentNullException(nameof(queryType));
            if (!queryType.GetTypeInfo().IsSubclassOf(typeof(Query))) throw new ArgumentOutOfRangeException(nameof(queryType));

            var info = Aggregates.SelectMany(x => x.Views).SelectMany(x => x.Queries).SingleOrDefault(x => x.Type == queryType);
            return info ?? throw new LanguageSchemaException($"No query {queryType.FullName} in language");
        }

        public AggregateViewInfo GetView(Type viewType)
        {
            if (viewType == null) throw new ArgumentNullException(nameof(viewType));
            if (!viewType.GetTypeInfo().IsSubclassOf(typeof(AggregateView))) throw new ArgumentOutOfRangeException(nameof(viewType));

            var info = Aggregates.SelectMany(x => x.Views).SingleOrDefault(x => x.Type == viewType);
            return info ?? throw new LanguageSchemaException($"No aggregate view for query {viewType.FullName} in language");
        }

        public Type FindCommand(String aggregateName, String commandName) => FindTerm(Aggregates.SelectMany(a => a.Commands), aggregateName, commandName);

        public Type FindDomainEvent(String aggregateName, String domainEventName) => FindTerm(Aggregates.SelectMany(a => a.DomainEvents), aggregateName, domainEventName);

        public Type FindQuery(String aggregateName, String viewName, String queryName)
        {
            var a = aggregateName.ToLowerInvariant();
            var v = viewName.ToLowerInvariant();
            var q = queryName.ToLowerInvariant();

            var aggs = Aggregates.Where(x => x.FullName.ToLowerInvariant().Contains(a)).ToArray();
            if (aggs.Length > 1) throw new LanguageSchemaException($"Many aggregates found by name {aggregateName}");
            if (aggs.Length == 0) throw new LanguageSchemaException($"No aggregate {aggregateName} in language");

            var views = aggs.Single().Views.Where(x => x.FullName.ToLowerInvariant().Contains(v)).ToArray();
            if (views.Length > 1) throw new LanguageSchemaException($"Many aggregate views found by name {viewName}");
            if (views.Length == 0) throw new LanguageSchemaException($"No aggregate view {viewName} in language");

            var queries = views.Single().Queries.Where(x => x.FullName.ToLowerInvariant().Contains(q)).ToArray();
            if (queries.Length > 1) throw new LanguageSchemaException($"Many queries found by name {queryName}");
            if (queries.Length == 0) throw new LanguageSchemaException($"No query {queryName} in language");

            return queries.Single().Type;
        }

        public Type FindDomainEvent(String eventFullName)
        {
            var e = eventFullName.ToLowerInvariant();
            var info = Aggregates.SelectMany(a => a.DomainEvents).SingleOrDefault(x => String.Equals(x.FullName, e, StringComparison.OrdinalIgnoreCase));
            return info?.Type ?? throw new LanguageSchemaException($"No event {eventFullName} in language");
        }

        private Type FindTerm(IEnumerable<MessageInfo> aggregateItems, String aggregateName, String aggregateItemName)
        {
            var a = aggregateName.ToLowerInvariant();
            var i = aggregateItemName.ToLowerInvariant();
            var aggregateItem = aggregateItems.SingleOrDefault(x => String.Equals(x.Aggregate.Name, a, StringComparison.OrdinalIgnoreCase) && String.Equals(x.Name, i, StringComparison.OrdinalIgnoreCase));
            return aggregateItem?.Type ?? throw new LanguageSchemaException($"No {aggregateItemName} in {aggregateName} aggregate");
        }
    }
}