using System;

namespace Cleanic.Core
{
    public class Command : ValueObject
    {
        public String AggregateId { get; set; }
    }
}