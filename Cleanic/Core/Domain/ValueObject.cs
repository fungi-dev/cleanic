using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cleanic.Core
{
    /// <summary>
    /// Domain object representing characteristic of some other object.
    /// </summary>
    public abstract class ValueObject : DomainObject
    {
        protected override IEnumerable<Object> GetIdentityComponents()
        {
            return GetType().GetRuntimeProperties()
                .Where(x => x.GetMethod?.IsStatic == false)
                .Select(x => x.GetValue(this));
        }
    }
}