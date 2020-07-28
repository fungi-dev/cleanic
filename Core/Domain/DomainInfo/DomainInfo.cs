using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Cleanic
{
    public class DomainInfo
    {
        public IReadOnlyCollection<AggregateLogicInfo> Aggregates { get; internal set; }
        public IReadOnlyCollection<SagaInfo> Sagas { get; internal set; }
        public IReadOnlyCollection<ServiceInfo> Services { get; internal set; }

        public AggregateLogicInfo GetAggregateLogic(AggregateInfo aggregateInfo)
        {
            var info = Aggregates.SingleOrDefault(x => x.AggregateInfo == aggregateInfo);
            return info ?? throw new Exception($"No aggregate logic for {aggregateInfo.Name} in domain");
        }

        public SagaInfo[] GetReactingSagas(EventInfo eventInfo) => Sagas.Where(x => x.Events.Contains(eventInfo)).ToArray();
    }
}