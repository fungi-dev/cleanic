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
            var type = typeof(T);
            if (!type.GetTypeInfo().IsSubclassOf(typeof(Aggregate))) throw new ArgumentOutOfRangeException("T");
            _aggregateInfos.Add(ProduceAggregateInfoFromType(type));

            return this;
        }

        public LanguageSchema Build()
        {
            return new LanguageSchema { Aggregates = _aggregateInfos.ToImmutableHashSet() };
        }

        private List<AggregateInfo> _aggregateInfos = new List<AggregateInfo>();

        private IEnumerable<Type> FindAggregateTypesInAssembly(Assembly languageAssembly) => languageAssembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(Aggregate)));

        private AggregateInfo ProduceAggregateInfoFromType(Type aggregateType)
        {
            var aggregateInfo = new AggregateInfo(aggregateType);

            var nested = aggregateType.GetTypeInfo().DeclaredNestedTypes;

            var commandTypes = nested.Where(x => x.IsSubclassOf(typeof(Command)));
            aggregateInfo.Commands = ProduceCommandInfosFromTypes(commandTypes, aggregateInfo).ToImmutableHashSet();

            var domainEventTypes = nested.Where(x => x.IsSubclassOf(typeof(DomainEvent)));
            aggregateInfo.DomainEvents = ProduceDomainEventInfosFromTypes(domainEventTypes, aggregateInfo).ToImmutableHashSet();

            var viewTypes = nested.Where(x => x.IsSubclassOf(typeof(AggregateView)));
            aggregateInfo.Views = ProduceViewInfosFromTypes(viewTypes, aggregateInfo).ToImmutableHashSet();
            foreach (var viewInfo in aggregateInfo.Views)
            {
                var queryTypes = viewInfo.Type.GetTypeInfo().DeclaredNestedTypes.Where(x => typeof(Query).GetTypeInfo().IsAssignableFrom(x));
                viewInfo.Queries = ProduceQueryInfosFromTypes(queryTypes, viewInfo).ToImmutableHashSet();
            }

            return aggregateInfo;
        }

        private IEnumerable<CommandInfo> ProduceCommandInfosFromTypes(IEnumerable<Type> commandTypes, AggregateInfo aggregateInfo)
        {
            return commandTypes.Select(x => new CommandInfo(x, aggregateInfo));
        }

        private IEnumerable<DomainEventInfo> ProduceDomainEventInfosFromTypes(IEnumerable<Type> domainEventTypes, AggregateInfo aggregateInfo)
        {
            return domainEventTypes.Select(x => new DomainEventInfo(x, aggregateInfo));
        }

        private IEnumerable<AggregateViewInfo> ProduceViewInfosFromTypes(IEnumerable<Type> viewTypes, AggregateInfo aggregateInfo)
        {
            return viewTypes.Select(x => new AggregateViewInfo(x, aggregateInfo));
        }

        private IEnumerable<QueryInfo> ProduceQueryInfosFromTypes(IEnumerable<Type> queryTypes, AggregateViewInfo viewInfo)
        {
            return queryTypes.Select(x => new QueryInfo(x, viewInfo));
        }
    }
}