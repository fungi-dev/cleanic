using Cleanic.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Cleanic
{
    public class DomainInfoBuilder
    {
        public DomainInfoBuilder(LanguageInfo languageInfo)
        {
            _languageInfo = languageInfo ?? throw new ArgumentNullException(nameof(languageInfo));
        }

        public DomainInfoBuilder AggregateLogic<TAggregate, TAggregateLogic>()
        {
            AddDomainTypesFromAssembly(typeof(TAggregateLogic).GetTypeInfo().Assembly);

            var aggregateInfo = _languageInfo.GetAggregate(typeof(TAggregate));
            var aggregateLogicType = typeof(TAggregateLogic);
            _aggregateTypes.Add(aggregateInfo, aggregateLogicType);

            return this;
        }

        public DomainInfoBuilder Saga<T>()
        {
            AddDomainTypesFromAssembly(typeof(T).GetTypeInfo().Assembly);

            _sagaTypes.Add(typeof(T));

            return this;
        }

        public void Build(DomainInfo domainInfo)
        {
            var aggregates = new List<AggregateLogicInfo>();
            var services = new Dictionary<Type, ServiceInfo>();
            foreach (var aggregate in _aggregateTypes)
            {
                var aggregateLogicInfo = new AggregateLogicInfo(aggregate.Value, aggregate.Key);

                var dependencies = new Dictionary<CommandInfo, List<ServiceInfo>>();
                var commandHandlersWithDependencies = aggregate.Value.GetTypeInfo().DeclaredMethods
                    .Where(m => m.GetParameters().Length > 1)
                    .Where(m => m.GetParameters().Any(p => p.ParameterType.GetTypeInfo().IsSubclassOf(typeof(Command))));
                foreach (var handler in commandHandlersWithDependencies)
                {
                    var commandParam = handler.GetParameters().Single(p => p.ParameterType.GetTypeInfo().IsSubclassOf(typeof(Command)));
                    var commandInfo = _languageInfo.GetCommand(commandParam.ParameterType);

                    var serviceParams = handler.GetParameters().Where(p =>
                    {
                        var t = p.ParameterType.GetTypeInfo();
                        if (t.IsArray) t = t.GetElementType().GetTypeInfo();
                        return t.IsSubclassOf(typeof(Service));
                    });
                    dependencies.Add(commandInfo, new List<ServiceInfo>());
                    foreach (var serviceType in serviceParams.Select(p => p.ParameterType))
                    {
                        var t = serviceType.IsArray ? serviceType.GetElementType() : serviceType;
                        if (!services.TryGetValue(t, out var serviceInfo)) services.Add(t, serviceInfo = new ServiceInfo(t));
                        dependencies[commandInfo].Add(serviceInfo);
                    }
                }
                aggregateLogicInfo.Dependencies = dependencies.ToImmutableDictionary(x => x.Key, x => (IReadOnlyCollection<ServiceInfo>)x.Value.ToImmutableHashSet());

                aggregates.Add(aggregateLogicInfo);
            }

            var sagas = new List<SagaInfo>();
            foreach (var sagaType in _sagaTypes)
            {
                var sagaInfo = new SagaInfo(sagaType);
                var events = sagaType.GetTypeInfo().DeclaredMethods
                    .SelectMany(m => m.GetParameters())
                    .Select(p => p.ParameterType)
                    .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(Event)))
                    .Distinct()
                    .Select(t => _languageInfo.GetEvent(t));
                sagaInfo.Events = events.ToImmutableHashSet();
                sagas.Add(sagaInfo);
            }

            domainInfo.Aggregates = aggregates.ToImmutableHashSet();
            domainInfo.Sagas = sagas.ToImmutableHashSet();
            domainInfo.Services = services.Values.ToImmutableHashSet();
        }

        private void AddDomainTypesFromAssembly(Assembly assembly)
        {
            var types = assembly.DefinedTypes.Where(x => x.IsSubclassOf(typeof(Service)) || x.IsSubclassOf(typeof(Saga)));
            foreach (var t in types) _domainTypes.Add(t.AsType());
        }

        private readonly LanguageInfo _languageInfo;
        private readonly HashSet<Type> _domainTypes = new HashSet<Type>();
        private readonly Dictionary<AggregateInfo, Type> _aggregateTypes = new Dictionary<AggregateInfo, Type>();
        private readonly List<Type> _sagaTypes = new List<Type>();
    }
}