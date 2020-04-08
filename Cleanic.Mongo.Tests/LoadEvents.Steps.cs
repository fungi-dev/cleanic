using Cleanic.Core;
using Shouldly;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cleanic.Framework.Tests
{
    public partial class LoadEvents : FeatureFixture
    {
        private String _aggregateId;
        private Event _event;
        private EventMeta _eventMeta;
        private AggregateMeta _aggregateMeta;

        private async Task When_there_is_aggregate_with_one_event()
        {
            _aggregateId = "FakeId";
            _aggregateMeta = new AggregateMeta(typeof(Agg).GetTypeInfo(), typeof(Agg).GetTypeInfo());
            _event = new Agg.Evt { AggregateId = _aggregateId };
            _eventMeta = new EventMeta(typeof(Agg.Evt).GetTypeInfo(), _aggregateMeta);

            await SUT.SaveEvents(_aggregateMeta.Name, _aggregateId, new[] { _event }, 0);
        }

        private async Task Then_repo_can_load_it_by_id()
        {
            var events = await SUT.LoadEvents(_aggregateMeta.Name, _aggregateId);
            events.ShouldNotBeNull();
            events.Length.ShouldBe(1);
            events.Single().ShouldBe(_event);
        }

        private async Task Then_repo_can_load_it_by_event_meta()
        {
            var events = await SUT.LoadEvents(new[] { _eventMeta });
            events.ShouldNotBeNull();
            events.Length.ShouldBe(1);
            events.Single().ShouldBe(_event);
        }

        public class Agg : Aggregate
        {
            public class Evt : Event { }
        }
    }
}