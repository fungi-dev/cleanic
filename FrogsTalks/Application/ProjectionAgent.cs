using FrogsTalks.Application.Ports;

namespace FrogsTalks.Application
{
    /// <summary>
    /// Agent behind the bus who updates domain projections when events occurred.
    /// </summary>
    /// <remarks>There can be many projection agent instances for one facade.</remarks>
    public abstract class ProjectionAgent
    {
        /// <summary>
        /// Create an instance of the application projection agent.
        /// </summary>
        /// <param name="bus">Bus to catch events.</param>
        /// <param name="db">Place to store built projections.</param>
        protected ProjectionAgent(IMessageBus bus, IProjectionsRepository db)
        {
            Bus = bus;
            Db = db;
        }

        /// <summary>
        /// Bus to catch events.
        /// </summary>
        protected readonly IMessageBus Bus;

        /// <summary>
        /// Place to store built projections.
        /// </summary>
        protected readonly IProjectionsRepository Db;
    }
}