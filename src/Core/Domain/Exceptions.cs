namespace Cleanic.Core
{
    using System;

    public class DomainSchemaException : Exception
    {
        public DomainSchemaException(String message) : base(message) { }
    }
}