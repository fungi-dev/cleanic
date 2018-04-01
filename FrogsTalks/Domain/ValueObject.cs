using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FrogsTalks.Domain
{
    /// <summary>
    /// Domain object representing characteristic of some other object.
    /// </summary>
    public abstract class ValueObject : DomainObject
    {
        protected override IEnumerable<Object> GetIdentityComponents()
        {
            var properties = GetType().GetTypeInfo().DeclaredProperties.Where(p => p.CanRead && p.CanWrite);
            var values = new List<Object>();
            foreach (var property in properties)
            {
                var value = property.GetValue(this);
                if (property.PropertyType == typeof(String))
                {
                    value = ((String)value)?.ToLowerInvariant();
                }
                values.Add(value);
            }
            return values;
        }
    }
}