using System;

namespace Cleanic.Domain
{
    /// <summary>
    /// The request of changes to the domain.
    /// </summary>
    public interface ICommand
    {
        String AggregateId { get; set; }
    }
}