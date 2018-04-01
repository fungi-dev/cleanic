using System;
using System.Linq;
using System.Reflection;
using FrogsTalks.Application.Ports;
using FrogsTalks.Domain;

namespace FrogsTalks.Application
{
    /// <summary>
    /// Agent behind the bus who handles user commands.
    /// </summary>
    /// <remarks>There can be many logic agent instances for one facade.</remarks>
    public class LogicAgent
    {
        /// <summary>
        /// Create an instance of the application logic agent.
        /// </summary>
        /// <param name="bus">Bus to catch commands and to publish events.</param>
        /// <param name="db">Storage to place produced events.</param>
        public LogicAgent(IMessageBus bus, IEventStore db)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _db = db ?? throw new ArgumentNullException(nameof(db));

            bus.HandleCommands(HandleCommand);
        }

        private readonly IMessageBus _bus;
        private readonly IEventStore _db;

        private void HandleCommand(Command cmd)
        {
            var cmdType = cmd.GetType().GetTypeInfo();
            var aggType = cmdType.BaseType.GenericTypeArguments[0];

            var agg = (Aggregate)Activator.CreateInstance(aggType, cmd.AggregateId);
            var persistedEvents = _db.Load(cmd.AggregateId);
            agg.LoadFromHistory(persistedEvents);
            
            var runner = cmdType.GetDeclaredMethod(nameof(Command<Aggregate>.Run));
            runner.Invoke(cmd, new Object[] { agg });

            var newEvents = agg.FreshChanges.ToArray();
            if (newEvents.Length > 0)
            {
                var persistedVersion = agg.Version - newEvents.Length;
                _db.Save(agg.Id, persistedVersion, newEvents);
            }

            foreach (var e in newEvents) _bus.Publish(e);
        }
    }
}