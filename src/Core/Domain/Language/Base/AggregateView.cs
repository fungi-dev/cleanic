namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;

    public abstract class AggregateView : DomainObject
    {
        public String AggregateId { get; set; }

        protected override IEnumerable<Object> GetIdentityComponents()
        {
            yield return AggregateId;
        }
    }
}