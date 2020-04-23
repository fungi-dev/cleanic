using System;

namespace Cleanic.Application
{
    public class BadDomainException : Exception
    {
        public Type DomainType { get; set; }

        public BadDomainException(Type domainType, String message) : base($"{message} ({domainType.FullName})")
        {
            DomainType = domainType;
        }

        public static BadDomainException NoProjectionBuilder(Type projectionType)
        {
            return new BadDomainException(projectionType, "No builder for projection");
        }

        public static BadDomainException NoCommandHandler(Type comandType)
        {
            return new BadDomainException(comandType, "No handler for command");
        }
    }
}