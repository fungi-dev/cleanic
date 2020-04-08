using System;

namespace Cleanic.Core
{
    public class Event : ValueObject
    {
        public String AggregateId { get; set; }
        public DateTime Moment { get; set; }
    }

    public class Error : Event { }
}