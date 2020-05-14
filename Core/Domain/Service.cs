using System;
using System.Collections.Generic;

namespace Cleanic.Core
{
    public abstract class Service : DomainObject
    {
        protected override IEnumerable<Object> GetIdentityComponents() => Array.Empty<Object>();
    }
}