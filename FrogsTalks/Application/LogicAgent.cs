using FrogsTalks.Application.Ports;
using FrogsTalks.Domain;
using FrogsTalks.UseCases;
using System;
using System.Linq;

namespace FrogsTalks.Application
{
    /// <summary>
    /// Agent behind the bus who handles user commands.
    /// </summary>
    /// <remarks>There can be many logic agent instances for one facade.</remarks>
    public abstract class LogicAgent
    {
        /// <summary>
        /// Create an instance of the application logic agent.
        /// </summary>
        /// <param name="bus">Bus to catch commands and to publish events.</param>
        /// <param name="db">Storage to place produced events.</param>
        protected LogicAgent(IMessageBus bus, IEventStore db)
        {
            _bus = bus;
            _db = db;

            bus.HandleCommands(c => HandleCommand(c));
        }

        /// <summary>
        /// Get aggregate associated with command.
        /// </summary>
        /// <param name="type">Command type.</param>
        /// <returns>Aggregate type.</returns>
        protected abstract Type GetAggregateTypeForCommand(Type type);

        /// <summary>
        /// Get command handler function.
        /// </summary>
        /// <param name="type">Command type.</param>
        /// <returns>Function taking aggregate instance with command data and returning produced events.</returns>
        protected abstract Func<Aggregate, ICommand, IEvent[]> GetHandlerForCommand(Type type);

        private readonly IMessageBus _bus;
        private readonly IEventStore _db;

        private void HandleCommand(ICommand c)
        {
            var handler = GetHandlerForCommand(c.GetType());
            var aggType = GetAggregateTypeForCommand(c.GetType());
            var agg = (Aggregate)Activator.CreateInstance(aggType);

            agg.Id = c.Id;
            var source = _db.Load(agg.Id);
            agg.ApplyEvents(source);

            var emitted = new IEvent[0];
            emitted = handler(agg, c).ToArray();
            foreach (var e in emitted) e.Id = agg.Id;

            if (emitted.Length > 0) _db.Save(agg.Id, agg.Version, emitted);

            foreach (var e in emitted) _bus.Publish(e);
        }
    }
}