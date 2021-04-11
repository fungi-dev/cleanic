namespace Cleanic.Core.Tests
{
    using Cleanic.Core;
    using System;
    using System.Threading.Tasks;

    #region Language

    public class DemoAgg : IAggregate
    {
        public class Cmd : Command { }

        public class View : AggregateView
        {
            public class Qr : Query { }
        }
    }

    #endregion

    #region Logic

    public class DemoAggLogic : AggregateLogic<DemoAgg>
    {
        public DemoAggLogic(String id) : base(id) { }

        public AggregateEvent Do(DemoAgg.Cmd cmd, DemoSvc svc) => new AggEvent();

        public class AggEvent : AggregateEvent { }
    }

    public class DemoSaga : Saga
    {
        public Task<Command[]> React(DemoAggLogic.AggEvent e) => Task.FromResult(Array.Empty<Command>());
    }

    public class DemoSvc : Service { }

    #endregion

    #region Projection

    public class DemoProjector : Projector<DemoAgg>
    {
        public void Apply(DemoAgg.View view, DemoAggLogic.AggEvent e) { }
    }

    #endregion
}