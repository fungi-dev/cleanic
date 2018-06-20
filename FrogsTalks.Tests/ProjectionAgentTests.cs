using System;
using FrogsTalks.Application;
using FrogsTalks.Application.Ports;
using FrogsTalks.Domain;
using NUnit.Framework;
using Shouldly;
using TestStack.BDDfy;

namespace FrogsTalks
{
    [Story(AsA = "As an application's read side team lead",
        IWant = "I want to be able to split event processing code",
        SoThat = "So that I can parallel the work")]
    [TestFixture]
    public class ProjectionAgentTests
    {
        [Test]
        public void MultipleEventHandlersInOneProjection()
        {
            this.Given(_ => _.ThereIsProjectionBuilderWithTwoEventApplierMethods())
                .And(_ => _.RequiredAdaptersAreCreated())
                .And(_ => _.ReadBackendIsCreated())
                .And(_ => _.ApplicationFacadeIsCreated())
                .When(_ => _.YouPublishSomeEvent())
                .Then(_ => _.NoExceptionHaveBeenRaised())
                .And(_ => _.UpdatedProjectionPersisted())
                .And(_ => _.BothMethodsHaveBeenCalled())
                .BDDfy();
        }

        #region Given steps

        private void ThereIsProjectionBuilderWithTwoEventApplierMethods()
        {
            _projections = new[] { typeof(TestProjection) };
        }

        private void RequiredAdaptersAreCreated()
        {
            _readDb = new InMemoryStateStore();
            _bus = new InMemoryBus();
        }

        private void ReadBackendIsCreated()
        {
            new ProjectionAgent(_bus, _readDb, _projections);
        }

        private void ApplicationFacadeIsCreated()
        {
            _app = new ApplicationFacade(_bus, _readDb);
        }

        #endregion

        #region When steps

        private void YouPublishSomeEvent()
        {
            try
            {
                _bus.Publish(new TestEvent { AggregateId = _1 });
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }

        #endregion

        #region Then steps

        private void NoExceptionHaveBeenRaised()
        {
            _exception.ShouldBeNull();
        }

        private void UpdatedProjectionPersisted()
        {
            var fromDb = _readDb.Load(_1) as TestProjection;
            var fromApp = _app.Get<TestProjection>(_1);
            fromDb.ShouldBe(fromApp);
            fromDb.ShouldNotBeNull();
            _projection = fromDb;
        }

        private void BothMethodsHaveBeenCalled()
        {
            _projection.FirstMethodCalled.ShouldBeTrue();
            _projection.SecondMethodCalled.ShouldBeTrue();
        }

        #endregion

        #region Shared data

        private Type[] _projections;
        private InMemoryStateStore _readDb;
        private InMemoryBus _bus;
        private ApplicationFacade _app;
        private Exception _exception;
        private TestProjection _projection;
        private const String _1 = "1";

        #endregion

        #region Test types

        private class TestEvent : Event { }

        private class TestProjection : Projection
        {
            public TestProjection(String id) : base(id) { }

            public Boolean FirstMethodCalled { get; private set; }
            public Boolean SecondMethodCalled { get; private set; }

            public void FirstApplier(TestEvent e)
            {
                FirstMethodCalled = true;
            }

            public void SecondApplier(TestEvent e)
            {
                SecondMethodCalled = true;
            }
        }

        #endregion
    }
}