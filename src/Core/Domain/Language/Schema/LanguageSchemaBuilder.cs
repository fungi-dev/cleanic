namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <remarks>
    /// Можно было не создавать отдельный класс для построения схемы, а при создании схемы, чтобы она сама сканила свою ассембли и находила агрегаты.
    /// Но это не позволяет запускать приложения, которые должны работать на основе ограниченной части предметки.
    /// А такая ситуация может возникнуть при определении отдельного приложения для построения особо нагруженной вьюхи, например.
    /// Билдер выделен в отдельный класс именно потому, что в подобных ситуациях процесс построения схемы состоит из нескольких шагов.
    /// </remarks>
    public class LanguageSchemaBuilder
    {
        public LanguageSchema BuildFromAssembly(Assembly assembly)
        {
            var types = FindAggregateTypesInAssembly(assembly);
            _aggregateInfos = types.Select(x => ProduceAggregateInfoFromType(x)).ToList();

            return Build();
        }

        public LanguageSchemaBuilder Add<T>()
        {
            _aggregateInfos.Add(ProduceAggregateInfoFromType(typeof(T)));
            return this;
        }

        public LanguageSchema Build()
        {
            return new LanguageSchema { Aggregates = _aggregateInfos.ToImmutableHashSet() };
        }

        private List<AggregateInfo> _aggregateInfos = new List<AggregateInfo>();

        private IEnumerable<Type> FindAggregateTypesInAssembly(Assembly languageAssembly) => languageAssembly.DefinedTypes.Where(x => x.ImplementedInterfaces.Contains(typeof(IAggregate)));

        private AggregateInfo ProduceAggregateInfoFromType(Type aggregateType)
        {
            var aggregateInfo = new AggregateInfo(aggregateType);

            var nested = aggregateType.GetTypeInfo().DeclaredNestedTypes;

            var commandTypes = nested.Where(x => x.IsSubclassOf(typeof(Command)));
            aggregateInfo.Commands = commandTypes.Select(x => new CommandInfo(x, aggregateInfo.IsRoot)).ToImmutableHashSet();

            var viewTypes = nested.Where(x => x.IsSubclassOf(typeof(AggregateView)));
            aggregateInfo.Views = viewTypes.Select(x => new AggregateViewInfo(x, aggregateInfo.IsRoot)).ToImmutableHashSet();
            foreach (var viewInfo in aggregateInfo.Views)
            {
                var queryTypes = viewInfo.Type.GetTypeInfo().DeclaredNestedTypes.Where(x => typeof(Query).GetTypeInfo().IsAssignableFrom(x));
                viewInfo.Queries = queryTypes.Select(x => new QueryInfo(x, aggregateInfo.IsRoot)).ToImmutableHashSet();
            }

            return aggregateInfo;
        }
    }
}