using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FrogsTalks.Domain;
using FrogsTalks.UseCases;

namespace FrogsTalks.CashMushroom
{
    #region Aggregates

    public class Product : Aggregate
    {
        public String Name { get; private set; }
        public Decimal Cost { get; private set; }
        public Friend Buyer { get; private set; }
        public IReadOnlyCollection<Friend> Payers { get; } = new List<Friend>();

        public IEnumerable Handle(RecordCosts c)
        {
            if (Version > 0) throw new ProductAlreadyPurchased();

            yield return new CostsRecorded
            {
                Id = c.Id,
                Name = c.Name,
                Cost = c.Cost,
                Buyer = c.Buyer,
                Payers = c.Payers
            };
        }

        public void Apply(CostsRecorded e)
        {
            Id = e.Id;
            Name = e.Name;
            Cost = e.Cost;
            Buyer = new Friend { Name = e.Buyer };
            ((List<Friend>)Payers).AddRange(e.Payers.Select(x => new Friend { Name = x }));
        }
    }

    public class Friend : ValueObject
    {
        public String Name { get; set; }
    }

    public class CostsRecorded : IEvent
    {
        public Guid Id { get; set; }
        public String Name { get; set; }
        public Decimal Cost { get; set; }
        public String Buyer { get; set; }
        public String[] Payers { get; set; }
    }

    public class ProductAlreadyPurchased : Exception { }

    #endregion

    #region Projections

    public class Bill : IProjection
    {
        public Guid Id { get; set; }
        public List<Party> Parties { get; set; } = new List<Party>();

        public class Party
        {
            public String Name { get; set; }
            public Decimal Total { get; set; }
        }
    }

    #endregion

    #region UseCases

    public class RecordCosts : ICommand
    {
        public Guid Id { get; set; }
        public String Name { get; set; }
        public Decimal Cost { get; set; }
        public String Buyer { get; set; }
        public String[] Payers { get; set; }
    }

    #endregion

    #region Constants

    public class Tenant
    {
        public static Guid Id = Guid.Parse("FAB82032-080A-4D37-9E62-DA9677043995");
    }

    #endregion
}