namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;

    public abstract class AggregateView : Message
    {
        protected override IEnumerable<Object> GetIdentityComponents()
        {
            yield return AggregateId;
        }
    }
}