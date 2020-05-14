using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Cleanic.Application
{
    public class DomainInfo
    {
        public IReadOnlyCollection<AggregateLogicInfo> Aggregates { get; }
        public IReadOnlyCollection<SagaInfo> Sagas { get; }
        public IReadOnlyCollection<ServiceInfo> Services { get; }

        public DomainInfo(IEnumerable<AggregateLogicInfo> aggregates, IEnumerable<SagaInfo> sagas, IEnumerable<ServiceInfo> services)
        {
            Aggregates = aggregates.ToImmutableHashSet();
            Sagas = sagas.ToImmutableHashSet();
            Services = services.ToImmutableHashSet();
        }

        public AggregateLogicInfo GetAggregateLogic(AggregateInfo aggregateInfo)
        {
            var info = Aggregates.SingleOrDefault(x => x.AggregateInfo == aggregateInfo);
            return info ?? throw new Exception($"No aggregate logic for {aggregateInfo.Name} in domain");
        }

        public SagaInfo[] GetReactingSagas(EventInfo eventInfo) => Sagas.Where(x => x.Events.Contains(eventInfo)).ToArray();
    }
}