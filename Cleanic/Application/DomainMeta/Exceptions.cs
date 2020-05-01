using System;

namespace Cleanic.Application
{
    public class BadDomainException : Exception
    {
        public BadDomainException(Type domainObjectType, String message) : base($"{message} ({domainObjectType.FullName})") { }

        public static BadDomainException NoProjectionBuilder(Type projectionType)
        {
            return new BadDomainException(projectionType, "No builder for projection");
        }

        public static BadDomainException NoCommandHandler(Type comandType)
        {
            return new BadDomainException(comandType, "No handler for command");
        }

        public static BadDomainException NoInternalAggregateClass(Type containerAggregateType)
        {
            var message = @"Aggregate should be implemented by two classes. 
                            Declaration class – container for commands, events, projections. 
                            And implementation class containing command handlers and event appliers. 
                            There is no implementation class for given declaration";
            return new BadDomainException(containerAggregateType, message);
        }
    }

    public class PoorDomainException : Exception
    {
        public PoorDomainException(Type domainObjectType, String message) : base($"{message} ({domainObjectType.FullName})") { }

        public PoorDomainException(String domainObjectMetaName, String message) : base($"{message} ({domainObjectMetaName})") { }

        public static PoorDomainException NoCommand(Type commandType)
        {
            return new PoorDomainException(commandType, "No such command registered in the domain");
        }

        public static PoorDomainException NoEvent(Type eventType)
        {
            return new PoorDomainException(eventType, "No such event registered in the domain");
        }

        public static PoorDomainException NoEvent(String eventMetaName)
        {
            return new PoorDomainException(eventMetaName, "No such event registered in the domain");
        }

        public static PoorDomainException NoProjection(Type projectionType)
        {
            return new PoorDomainException(projectionType, "No such projection registered in the domain");
        }

        public static PoorDomainException NoAggregate(Type aggregateType)
        {
            return new PoorDomainException(aggregateType, "No such aggregate registered in the domain");
        }
    }
}