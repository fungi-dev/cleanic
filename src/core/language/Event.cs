using System;

namespace Cleanic.Core
{
    public class Event : Message
    {
        public DateTime EventOccurred { get; set; }
    }

    public class Error : Event { }
}