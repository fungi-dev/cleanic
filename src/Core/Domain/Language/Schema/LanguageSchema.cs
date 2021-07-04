namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class LanguageSchema
    {
        public IReadOnlyCollection<EntityInfo> Entities { get; internal set; }

        public LanguageSchema()
        {
            Entities = Array.Empty<EntityInfo>().ToImmutableHashSet();
        }

        public EntityInfo GetEntity(Type entityType)
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));
            if (!entityType.IsSubclassOf(typeof(Entity))) throw new ArgumentOutOfRangeException(nameof(entityType));

            var info = Entities.SingleOrDefault(x => x.Type == entityType);
            return info ?? throw new LanguageSchemaException($"No entity '{entityType.FullName}' found in domain language");
        }
        public EntityInfo GetEntity<T>() where T : Entity => GetEntity(typeof(T));

        public MessageInfo GetMessage(Type messageType)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));
            if (!messageType.IsSubclassOf(typeof(Command)) && !messageType.IsSubclassOf(typeof(Query)) && !messageType.IsSubclassOf(typeof(View))) throw new ArgumentOutOfRangeException(nameof(messageType));

            var commandInfo = Entities.SelectMany(x => x.Commands).SingleOrDefault(x => x.Type == messageType);
            var queryInfo = Entities.SelectMany(x => x.Views).SelectMany(x => x.Queries).SingleOrDefault(x => x.Type == messageType);
            var viewInfo = Entities.SelectMany(x => x.Views).SingleOrDefault(x => x.Type == messageType);
            if (commandInfo == null && queryInfo == null && viewInfo == null) throw new LanguageSchemaException($"No message '{messageType.FullName}' found in domain language");

            return commandInfo ?? (MessageInfo)queryInfo ?? viewInfo;
        }
        public MessageInfo GetMessage<T>() where T : Message => GetMessage(typeof(T));

        public CommandInfo GetCommand(Type commandType)
        {
            if (commandType == null) throw new ArgumentNullException(nameof(commandType));
            if (!commandType.IsSubclassOf(typeof(Command))) throw new ArgumentOutOfRangeException(nameof(commandType));

            var info = Entities.SelectMany(x => x.Commands).SingleOrDefault(x => x.Type == commandType);
            return info ?? throw new LanguageSchemaException($"No command '{commandType.FullName}' found in domain language");
        }
        public CommandInfo GetCommand<T>() where T : Command => GetCommand(typeof(T));

        public QueryInfo GetQuery(Type queryType)
        {
            if (queryType == null) throw new ArgumentNullException(nameof(queryType));
            if (!queryType.IsSubclassOf(typeof(Query))) throw new ArgumentOutOfRangeException(nameof(queryType));

            var info = Entities.SelectMany(x => x.Views).SelectMany(x => x.Queries).SingleOrDefault(x => x.Type == queryType);
            return info ?? throw new LanguageSchemaException($"No query '{queryType.FullName}' found in domain language");
        }
        public QueryInfo GetQuery<T>() where T : Query => GetQuery(typeof(T));

        public ViewInfo GetView(Type viewType)
        {
            if (viewType == null) throw new ArgumentNullException(nameof(viewType));
            if (!viewType.IsSubclassOf(typeof(View))) throw new ArgumentOutOfRangeException(nameof(viewType));

            var info = Entities.SelectMany(x => x.Views).SingleOrDefault(x => x.Type == viewType);
            return info ?? throw new LanguageSchemaException($"No view for query '{viewType.FullName}' found in domain language");
        }
        public ViewInfo GetView<T>() where T : View => GetView(typeof(T));


        public ViewInfo GetView(QueryInfo queryInfo)
        {
            if (queryInfo == null) throw new ArgumentNullException(nameof(queryInfo));

            var info = Entities.SelectMany(x => x.Views).SingleOrDefault(x => x.Queries.Contains(queryInfo));
            return info ?? throw new LanguageSchemaException($"No view for query '{queryInfo.Name}' found in domain language");
        }

        public EntityInfo FindEntity(String entityName)
        {
            if (String.IsNullOrEmpty(entityName)) throw new ArgumentNullException(nameof(entityName));

            var aggs = Entities.Where(x => String.Equals(x.Name, entityName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (aggs.Length > 1) throw new LanguageSchemaException($"Many entities named '{entityName}' found in domain language");
            if (aggs.Length == 0) throw new LanguageSchemaException($"No entity '{entityName}' found in domain language");

            return aggs.Single();
        }

        public CommandInfo FindCommand(String entityName, String commandName)
        {
            if (String.IsNullOrEmpty(entityName)) throw new ArgumentNullException(nameof(entityName));
            if (String.IsNullOrEmpty(commandName)) throw new ArgumentNullException(nameof(commandName));

            var agg = FindEntity(entityName);

            var commands = agg.Commands.Where(x => String.Equals(x.Name, commandName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (commands.Length > 1) throw new LanguageSchemaException($"Many commands named '{commandName}' found in domain language");
            if (commands.Length == 0) throw new LanguageSchemaException($"No command '{commandName}' found in domain language");

            return commands.Single();
        }

        public ViewInfo FindView(String entityName, String viewName)
        {
            if (String.IsNullOrEmpty(entityName)) throw new ArgumentNullException(nameof(entityName));
            if (String.IsNullOrEmpty(viewName)) throw new ArgumentNullException(nameof(viewName));

            var agg = FindEntity(entityName);

            var views = agg.Views.Where(x => String.Equals(x.Name, viewName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (views.Length > 1) throw new LanguageSchemaException($"Many views named '{viewName}' found in domain language");
            if (views.Length == 0) throw new LanguageSchemaException($"No view '{viewName}' found in domain language");

            return views.Single();
        }

        public QueryInfo FindQuery(String entityName, String viewName, String queryName)
        {
            if (String.IsNullOrEmpty(entityName)) throw new ArgumentNullException(nameof(entityName));
            if (String.IsNullOrEmpty(viewName)) throw new ArgumentNullException(nameof(viewName));
            if (String.IsNullOrEmpty(queryName)) throw new ArgumentNullException(nameof(queryName));

            var view = FindView(entityName, viewName);

            var queries = view.Queries.Where(x => String.Equals(x.Name, queryName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (queries.Length > 1) throw new LanguageSchemaException($"Many queries named '{queryName}' found in domain language");
            if (queries.Length == 0) throw new LanguageSchemaException($"No query '{queryName}' found in domain language");

            return queries.Single();
        }
    }
}