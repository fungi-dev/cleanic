using System;

namespace FrogsTalks.UseCases
{
    /// <summary>
    /// The request of changes to the domain.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// The unique identifier of the aggregate.
        /// </summary>
        Guid Id { get; set; }
    }
}