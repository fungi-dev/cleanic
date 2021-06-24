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
            if (!aggregateType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IAggregate))) throw new ArgumentOutOfRangeException(nameof(aggregateType));

            var info = Aggregates.SingleOrDefault(x => x.Type == aggregateType);
            return info ?? throw new LanguageSchemaException($"No aggregate '{aggregateType.FullName}' found in domain language");
        }
        public AggregateInfo GetAggregate<T>() where T : IAggregate => GetAggregate(typeof(T));


        public MessageInfo GetMessage(Type messageType)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));
            if (!messageType.GetTypeInfo().IsSubclassOf(typeof(Command)) && !messageType.GetTypeInfo().IsSubclassOf(typeof(Query)) && !messageType.GetTypeInfo().IsSubclassOf(typeof(AggregateView))) throw new ArgumentOutOfRangeException(nameof(messageType));

            var commandInfo = Aggregates.SelectMany(x => x.Commands).SingleOrDefault(x => x.Type == messageType);
            var queryInfo = Aggregates.SelectMany(x => x.Views).SelectMany(x => x.Queries).SingleOrDefault(x => x.Type == messageType);
            var viewInfo = Aggregates.SelectMany(x => x.Views).SingleOrDefault(x => x.Type == messageType);
            if (commandInfo == null && queryInfo == null && viewInfo == null) throw new LanguageSchemaException($"No message '{messageType.FullName}' found in domain language");

            return commandInfo ?? (MessageInfo)queryInfo ?? viewInfo;
        }
        public MessageInfo GetMessage<T>() where T : Message => GetMessage(typeof(T));

        public CommandInfo GetCommand(Type commandType)
        {
            if (commandType == null) throw new ArgumentNullException(nameof(commandType));
            if (!commandType.GetTypeInfo().IsSubclassOf(typeof(Command))) throw new ArgumentOutOfRangeException(nameof(commandType));

            var info = Aggregates.SelectMany(x => x.Commands).SingleOrDefault(x => x.Type == commandType);
            return info ?? throw new LanguageSchemaException($"No command '{commandType.FullName}' found in domain language");
        }
        public CommandInfo GetCommand<T>() where T : Command => GetCommand(typeof(T));

        public QueryInfo GetQuery(Type queryType)
        {
            if (queryType == null) throw new ArgumentNullException(nameof(queryType));
            if (!queryType.GetTypeInfo().IsSubclassOf(typeof(Query))) throw new ArgumentOutOfRangeException(nameof(queryType));

            var info = Aggregates.SelectMany(x => x.Views).SelectMany(x => x.Queries).SingleOrDefault(x => x.Type == queryType);
            return info ?? throw new LanguageSchemaException($"No query '{queryType.FullName}' found in domain language");
        }
        public QueryInfo GetQuery<T>() where T : Query => GetQuery(typeof(T));

        public AggregateViewInfo GetAggregateView(Type viewType)
        {
            if (viewType == null) throw new ArgumentNullException(nameof(viewType));
            if (!viewType.GetTypeInfo().IsSubclassOf(typeof(AggregateView))) throw new ArgumentOutOfRangeException(nameof(viewType));

            var info = Aggregates.SelectMany(x => x.Views).SingleOrDefault(x => x.Type == viewType);
            return info ?? throw new LanguageSchemaException($"No aggregate view for query '{viewType.FullName}' found in domain language");
        }
        public AggregateViewInfo GetAggregateView<T>() where T : AggregateView => GetAggregateView(typeof(T));


        public AggregateViewInfo GetAggregateView(QueryInfo queryInfo)
        {
            if (queryInfo == null) throw new ArgumentNullException(nameof(queryInfo));

            var info = Aggregates.SelectMany(x => x.Views).SingleOrDefault(x => x.Queries.Contains(queryInfo));
            return info ?? throw new LanguageSchemaException($"No aggregate view for query '{queryInfo.Name}' found in domain language");
        }

        public AggregateInfo FindAggregate(String aggregateName)
        {
            if (String.IsNullOrEmpty(aggregateName)) throw new ArgumentNullException(nameof(aggregateName));

            var aggs = Aggregates.Where(x => String.Equals(x.Name, aggregateName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (aggs.Length > 1) throw new LanguageSchemaException($"Many aggregates named '{aggregateName}' found in domain language");
            if (aggs.Length == 0) throw new LanguageSchemaException($"No aggregate '{aggregateName}' found in domain language");

            return aggs.Single();
        }

        public CommandInfo FindCommand(String aggregateName, String commandName)
        {
            if (String.IsNullOrEmpty(aggregateName)) throw new ArgumentNullException(nameof(aggregateName));
            if (String.IsNullOrEmpty(commandName)) throw new ArgumentNullException(nameof(commandName));

            var agg = FindAggregate(aggregateName);

            var commands = agg.Commands.Where(x => String.Equals(x.Name, commandName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (commands.Length > 1) throw new LanguageSchemaException($"Many commands named '{commandName}' found in domain language");
            if (commands.Length == 0) throw new LanguageSchemaException($"No command '{commandName}' found in domain language");

            return commands.Single();
        }

        public AggregateViewInfo FindAggregateView(String aggregateName, String viewName)
        {
            if (String.IsNullOrEmpty(aggregateName)) throw new ArgumentNullException(nameof(aggregateName));
            if (String.IsNullOrEmpty(viewName)) throw new ArgumentNullException(nameof(viewName));

            var agg = FindAggregate(aggregateName);

            var views = agg.Views.Where(x => String.Equals(x.Name, viewName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (views.Length > 1) throw new LanguageSchemaException($"Many aggregate views named '{viewName}' found in domain language");
            if (views.Length == 0) throw new LanguageSchemaException($"No aggregate view '{viewName}' found in domain language");

            return views.Single();
        }

        public QueryInfo FindQuery(String aggregateName, String viewName, String queryName)
        {
            if (String.IsNullOrEmpty(aggregateName)) throw new ArgumentNullException(nameof(aggregateName));
            if (String.IsNullOrEmpty(viewName)) throw new ArgumentNullException(nameof(viewName));
            if (String.IsNullOrEmpty(queryName)) throw new ArgumentNullException(nameof(queryName));

            var view = FindAggregateView(aggregateName, viewName);

            var queries = view.Queries.Where(x => String.Equals(x.Name, queryName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (queries.Length > 1) throw new LanguageSchemaException($"Many queries named '{queryName}' found in domain language");
            if (queries.Length == 0) throw new LanguageSchemaException($"No query '{queryName}' found in domain language");

            return queries.Single();
        }
    }
}