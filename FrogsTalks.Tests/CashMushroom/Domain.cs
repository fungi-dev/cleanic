using System;
using System.Collections.Generic;
using System.Linq;
using FrogsTalks.Domain;

namespace FrogsTalks.CashMushroom
{
    #region Aggregates

    public class Product : Aggregate
    {
        #region Command handlers

        public void Do(RecordCosts c)
        {
            if (Version > 0) throw new ProductAlreadyPurchased();

            Apply(new ProductPurchased
            {
                Id = c.Id,
                Name = c.Name
            });

            Apply(new CostsRecorded
            {
                Id = c.Id,
                Cost = c.Cost,
                Buyer = c.Buyer,
                Payers = c.Payers
            });
        }

        #endregion

        #region State

        public void On(ProductPurchased e)
        {
            Id = e.Id;
        }

        #endregion

        #region DomainObjects

        public class Friend : ValueObject
        {
            public String Name { get; set; }
        }

        #endregion

        #region Commands

        public class RecordCosts : ICommand
        {
            public Guid Id { get; set; }
            public String Name { get; set; }
            public Decimal Cost { get; set; }
            public String Buyer { get; set; }
            public String[] Payers { get; set; }
        }

        #endregion

        #region Events

        public class ProductPurchased : InitialEvent
        {
            public Guid Id { get; set; }
            public String Name { get; set; }
        }

        public class CostsRecorded : Event
        {
            public Guid Id { get; set; }
            public Decimal Cost { get; set; }
            public String Buyer { get; set; }
            public String[] Payers { get; set; }
        }

        public class ProductAlreadyPurchased : Exception { }

        #endregion
    }

    #endregion

    #region Projections

    public class Bill : IProjection
    {
        public Guid Id { get; set; }
        public List<Party> Parties { get; } = new List<Party>();

        public class Party
        {
            public String Name { get; set; }
            public Decimal Total { get; set; }
        }

        public void On(Product.CostsRecorded e)
        {
            foreach (var name in e.Payers)
            {
                var payer = Parties.SingleOrDefault(x => x.Name == name);
                if (payer == null) Parties.Add(payer = new Party { Name = name });
                payer.Total += e.Cost / e.Payers.Length;
            }
        }

        public static Guid GetId(Product.CostsRecorded e)
        {
            return Tenant.Id;
        }
    }

    #endregion

    #region Constants

    public static class Tenant
    {
        public static Guid Id = Guid.Parse("FAB82032-080A-4D37-9E62-DA9677043995");
    }

    #endregion
}