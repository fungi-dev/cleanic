using System;

namespace Cleanic
{
    public class Configuration
    {
        public Type[] ProjectionsToMaterialize { get; set; } = Array.Empty<Type>();
    }
}