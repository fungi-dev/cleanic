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
            _writeDb = new InMemoryEventStore();
            _readDb = new InMemoryStateStore();
            _bus = new InMemoryBus();
        }

        private void WriteBackendIsCreated()
        {
            new Logic(_bus, _writeDb);
        }

        private void ReadBackendIsCreated()
        {
            new Projections(_bus, _readDb);
        }

        private void ApplicationFacadeIsCreated()
        {
            _app = new CashMushroom.Application(_bus, _readDb);
        }

        #endregion

        #region When steps

        private void YouSendSomeCommand()
        {
            var cmd = new RecordCosts
            {
                Id = _1,
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
            var bill = _app.Get<Bill>(Tenant.Id);
            var bobPart = bill.Parties.Single(x => x.Name == _bob);
            bobPart.Total.ShouldBe(_2k);
            var otherParts = bill.Parties.Where(x => x.Name != _bob);
            otherParts.Sum(x => x.Total).ShouldBe(0);
        }

        #endregion

        #region Shared data

        private InMemoryEventStore _writeDb;
        private InMemoryStateStore _readDb;
        private InMemoryBus _bus;
        private ApplicationFacade _app;

        private readonly Guid _1 = Guid.NewGuid();
        private const String _whiskey = "Jack Daniel's";
        private const Decimal _2k = 2000;
        private const String _bob = "Bob";

        #endregion
    }
}