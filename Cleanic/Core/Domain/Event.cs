using System;

namespace Cleanic.Core
{
    public class Event : ValueObject
    {
        public String AggregateId { get; set; }
        public DateTime EventOccurred { get; set; }
    }

    public class Error : Event { }
}