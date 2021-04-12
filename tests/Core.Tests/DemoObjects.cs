namespace Cleanic.Core.Tests
{
    using Cleanic.Core;
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    #region Language

    [Guid("F50A723B-6874-49F9-AF68-6D571C4FA07B")]
    public class DemoAgg : IAggregate
    {
        [Guid("7EE341BD-B781-44AE-A9A5-1CBA066D91CE")]
        public class Cmd : Command { }

        [Guid("0120F049-A29A-4EC7-84F4-2CA3E56649CB")]
        public class View : AggregateView
        {
            [Guid("C8CC8E31-552F-402C-ABE3-DF98A8341A2E")]
            public class Qr : Query { }
        }
    }

    #endregion

    #region Logic

    [Guid("F50A723B-6874-49F9-AF68-6D571C4FA07B")]
    public class DemoAggLogic : AggregateLogic<DemoAgg>
    {
        public DemoAggLogic(String id) : base(id) { }

        public AggregateEvent Do(DemoAgg.Cmd cmd, DemoSvc svc) => new AggEvent();

        [Guid("D53500FE-6E64-49BC-8271-3CE724CA0768")]
        public class AggEvent : AggregateEvent { }
    }

    [Guid("93942A98-7763-457D-A372-B80D8D3EE6C1")]
    public class DemoSaga : Saga
    {
        public Task<Command[]> React(DemoAggLogic.AggEvent e) => Task.FromResult(Array.Empty<Command>());
    }

    [Guid("21AAB50D-1DA1-427C-9954-06587F8FB856")]
    public class DemoSvc : Service { }

    #endregion

    #region Projection

    [Guid("30DEF6E3-DAEE-4C18-B907-882717EC64DB")]
    public class DemoProjector : Projector<DemoAgg>
    {
        public void Apply(DemoAgg.View view, DemoAggLogic.AggEvent e) { }
    }

    #endregion
}