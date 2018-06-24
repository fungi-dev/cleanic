using System;
using System.Linq;
using FrogsTalks.Application;
using FrogsTalks.Application.Ports;
using FrogsTalks.CashMushroom;
using NUnit.Framework;
using Shouldly;
using TestStack.BDDfy;

namespace FrogsTalks
{
    [Story(AsA = "As an application programmer",
        IWant = "I want to integrate my domain code with application logic code",
        SoThat = "So that I provide a simple endpoint to work with application for frontend programmers")]
    [TestFixture]
    public class ApplicationTests
    {
        [Test]
        public void TheSimpliestUseCase()
        {
            this.Given(_ => _.RequiredAdaptersAreCreated())
                .And(_ => _.WriteBackendIsCreated())
                .And(_ => _.ReadBackendIsCreated())
                .And(_ => _.ApplicationFacadeIsCreated())
                .When(_ => _.YouSendSomeCommand())
                .Then(_ => _.AppropriateProjectionIsBuilt())
                .BDDfy();
        }

        #region Given steps

        private void RequiredAdaptersAreCreated()
        {
            _db = new Repository(new InMemoryEventStore(), new InMemoryStateStore());
            _bus = new InMemoryBus();
        }

        private void WriteBackendIsCreated()
        {
            new LogicAgent(_bus, _db, typeof(Product));
        }

        private void ReadBackendIsCreated()
        {
            new ProjectionAgent(_bus, _db, typeof(Bill));
        }

        private void ApplicationFacadeIsCreated()
        {
            _app = new ApplicationFacade(_bus, _db);
        }

        #endregion

        #region When steps

        private void YouSendSomeCommand()
        {
            var cmd = new Product.RecordCosts
            {
                AggregateId = _1,
                Buyer = _bob,
                Payers = new[] { _bob },
                Cost = _2k,
                Name = _whiskey
            };
            _app.Do(cmd);
        }

        #endregion

        #region Then steps

        private void AppropriateProjectionIsBuilt()
        {
            var bill = _app.Get<Bill>(Constants.TenantId);
            var bobPart = bill.Parties.Single(x => x.Name == _bob);
            bobPart.Total.ShouldBe(_2k);
            var otherParts = bill.Parties.Where(x => x.Name != _bob);
            otherParts.Sum(x => x.Total).ShouldBe(0);
        }

        #endregion

        #region Shared data

        private Repository _db;
        private InMemoryBus _bus;
        private ApplicationFacade _app;

        private const String _1 = "1";
        private const String _whiskey = "Jack Daniel's";
        private const Decimal _2k = 2000;
        private const String _bob = "Bob";

        #endregion
    }
}