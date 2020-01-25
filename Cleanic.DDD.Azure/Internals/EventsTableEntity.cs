using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Cleanic.Framework
{
    internal class EventsTableEntity : TableEntity
    {
        public String Event { get; set; }
    }
}