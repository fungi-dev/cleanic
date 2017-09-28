using System;

namespace FrogsTalks.Domain
{
    /// <summary>
    /// Information about the state of the domain.
    /// </summary>
    public interface IProjection
    {
        /// <summary>
        /// The unique identifier of the aggregate.
        /// </summary>
        /// <remarks>
        /// If you think that there are no aggregate associated to this projection
        /// maybe it is some sort of global one (tenant, company, world).
        /// </remarks>
        Guid Id { get; set; }
    }
}