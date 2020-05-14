using System;
using System.Collections.Generic;

namespace Cleanic.Application
{
    public class Configuration
    {
        public IEnumerable<Type> ProjectionsToMaterialize { get; } = Array.Empty<Type>();
    }
}