namespace Cleanic.Core
{
    using System;

    /// <summary>
    /// Any thing in the domain.
    /// Actor can send commands to entity or do queries the state of each entity in the domain.
    /// </summary>
    public abstract class Entity : DomainObject
    {
        public String Id { get; protected set; }
    }
}