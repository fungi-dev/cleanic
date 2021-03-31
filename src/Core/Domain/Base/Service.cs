namespace Cleanic.Core
{
    using System;
    using System.Collections.Generic;

    public abstract class Service : DomainObject
    {
        protected override IEnumerable<Object> GetIdentityComponents() => Array.Empty<Object>();
    }
}