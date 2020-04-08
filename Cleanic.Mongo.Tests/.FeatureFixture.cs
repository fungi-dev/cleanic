using System;

namespace Cleanic.Framework.Tests
{
    public abstract class FeatureFixture : LightBDD.XUnit2.FeatureFixture, IDisposable
    {
        protected FeatureFixture()
        {
            SUT = new MongoEventStore();
        }

        public void Dispose()
        {
            SUT.Clear();
        }

        protected MongoEventStore SUT { get; }
    }
}