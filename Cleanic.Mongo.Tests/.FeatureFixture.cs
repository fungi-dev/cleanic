using System;

namespace Cleanic.Framework.Tests
{
    public abstract class FeatureFixture : LightBDD.XUnit2.FeatureFixture, IDisposable
    {
        protected FeatureFixture()
        {
            SUT = new MongoEventStore("mongodb+srv://admin:cVZelc4Uu6iMXGxj@alfacontext-db-af1cu.azure.mongodb.net?retryWrites=true&w=majority");
        }

        public void Dispose()
        {
            SUT.Clear();
        }

        protected MongoEventStore SUT { get; }
    }
}