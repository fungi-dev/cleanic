using System;
using System.Collections.Generic;
using System.Linq;
using FrogsTalks.Domain;

namespace FrogsTalks.CashMushroom
{
    public static class Constants
    {
        public static String TenantId = "the one";
    }

    #region Aggregates

    public class Product : Aggregate
    {
        public Product(String id) : base(id) { }

        #region Behaviour

        public void Do(RecordCosts cmd)
        {
            if (Version == 0) Apply(new ProductPurchased { AggregateId = Id, Name = cmd.Name });

            Apply(new CostsRecorded
            {
                AggregateId = Id,
                Cost = cmd.Cost,
                Buyer = cmd.Buyer,
                Payers = cmd.Payers.ToArray()
            });
        }

        #endregion

        #region State

        public String Name { get; private set; }

        public void On(ProductPurchased e)
        {
            Name = e.Name;
        }

        #endregion

        #region DomainObjects

        public class Friend : ValueObject
        {
            public String Name { get; set; }
        }

        public class RecordCosts : Command
        {
            public String Name { get; set; }
            public Decimal Cost { get; set; }
            public String Buyer { get; set; }
            public String[] Payers { get; set; }
        }

        #endregion

        #region Events

        public class ProductPurchased : InitialEvent
        {
            public String Name { get; set; }
        }

        public class CostsRecorded : Event
        {
            public Decimal Cost { get; set; }
            public String Buyer { get; set; }
            public String[] Payers { get; set; }
        }

        public class ProductAlreadyPurchased : Exception { }

        public class ProductIsNotPurchasedYet : Exception { }

        #endregion
    }

    #endregion

    #region Projections

    public class Bill : Projection
    {
        public Bill(String id) : base(id) { }

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

        public static String GetId(Product.CostsRecorded e)
        {
            return Constants.TenantId;
        }
    }

    #endregion
}