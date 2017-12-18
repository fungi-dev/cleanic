using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using FrogsTalks.Application;
using FrogsTalks.Application.Ports;
using FrogsTalks.Domain;
using FrogsTalks.UseCases;

namespace FrogsTalks.CashMushroom
{
    public class Application : ApplicationFacade
    {
        public Application(IMessageBus bus, IProjectionsReader projections) : base(bus, projections) { }
    }

    public class Logic : LogicAgent
    {
        public Logic(IMessageBus bus, IEventStore db) : base(bus, db) { }

        protected override Type GetAggregateTypeForCommand(Type type)
        {
            switch (type.Name)
            {
                case nameof(RecordCosts):
                    return typeof(Product);
                default:
                    throw new NotImplementedException();
            }
        }

        protected override Func<Aggregate, ICommand, IEvent[]> GetHandlerForCommand(Type type)
        {
            switch (type.Name)
            {
                case nameof(RecordCosts):
                    return (agg, cmd) =>
                    {
                        var handler = typeof(Product).GetTypeInfo().GetMethod(nameof(Product.Handle));
                        var events = (IEnumerable)handler.Invoke(agg, new[] { cmd });
                        return events.OfType<IEvent>().ToArray();
                    };
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Projections : ProjectionAgent
    {
        public Projections(IMessageBus bus, IProjectionsRepository db) : base(bus, db)
        {
            Bus.ListenEvent(typeof(CostsRecorded), e => Handle((CostsRecorded)e));
        }

        public void Handle(CostsRecorded e)
        {
            var bill = (Bill)Db.Load(Tenant.Id) ?? new Bill { Id = Tenant.Id };
            foreach (var name in e.Payers)
            {
                var payer = bill.Parties.SingleOrDefault(x => x.Name == name);
                if (payer == null) bill.Parties.Add(payer = new Bill.Party { Name = name });
                payer.Total += e.Cost / e.Payers.Length;
            }
            Db.Save(bill);
        }
    }
}