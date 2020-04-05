using Cleanic.Core;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Framework.Tests
{
    public partial class LoadEvents : FeatureFixture
    {
        private IIdentity<Agg.Self> _aggregateId;
        private IEvent _event;
        private EventMeta _eventMeta;
        private AggregateMeta _aggregateMeta;

        private async Task When_there_is_aggregate_with_one_event()
        {
            _aggregateId = new Agg.Id("FakeId");
            _aggregateMeta = new AggregateMeta(typeof(Agg), null);
            _event = new Agg.Evt(_aggregateId);
            _eventMeta = new EventMeta(typeof(Agg.Evt), _aggregateMeta);

            await SUT.SaveEvents(_aggregateMeta.Name, _aggregateId.Value, new[] { _event }, 0);
        }

        private async Task Then_repo_can_load_it_by_id()
        {
            var events = await SUT.LoadEvents(_aggregateMeta.Name, _aggregateId.Value);
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

        public class Agg : Aggregate<Agg.Self>
        {
            public class Self : Entity<Self> { public Self(Id id) : base(id) { } }
            public class Evt : Event<Self> { public Evt(IIdentity<Self> id) : base(id) { } }
            public new class Id : Id<Self> { public Id(String value) : base(value) { } }
            public Agg(IIdentity id) : base(id) { }
        }
    }
}