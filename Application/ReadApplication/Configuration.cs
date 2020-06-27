using System;

namespace Cleanic.Application
{
    public class Configuration
    {
        public Type[] ProjectionsToMaterialize { get; set; } = Array.Empty<Type>();
    }
}