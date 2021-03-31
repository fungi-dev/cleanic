namespace Cleanic.Core
{
    using System;

    public abstract class Event1 : BehavioralTerm
    {
        public DateTime EventOccurred { get; set; }
    }
}