using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        /// <param name="aggregates">All aggregates which implement domain logic.</param>
        public LogicAgent(IMessageBus bus, Repository db, params Type[] aggregates)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            if (aggregates.Any(_ => !typeof(Aggregate).GetTypeInfo().IsAssignableFrom(_.GetTypeInfo())))
            {
                throw new ArgumentException("Aggregate types are expected but some other was found!");
            }
            _aggregates = aggregates;

            bus.HandleCommands(HandleCommand);
        }

        /// <summary>
        /// Get aggregate associated with command.
        /// </summary>
        /// <param name="commandType">Command type.</param>
        /// <returns>Aggregate type.</returns>
        protected virtual Type GetAggregateTypeForCommand(Type commandType)
        {
            foreach (var aggregate in _aggregates)
            {
                var nested = aggregate.GetTypeInfo().DeclaredNestedTypes;
                if (nested.Contains(commandType.GetTypeInfo())) return aggregate;
            }
            throw new Exception($"There is no aggregate for command \"{commandType}\" in the domain!");
        }

        /// <summary>
        /// Ask aggregate to handle command and get produced events.
        /// </summary>
        /// <param name="aggregate">Aggregate with all current events applied.</param>
        /// <param name="command">Command data.</param>
        /// <returns>New events which occurred while handling command.</returns>
        protected virtual Event[] ProduceEvents(Aggregate aggregate, Command command)
        {
            if (aggregate == null) throw new ArgumentNullException(nameof(aggregate));
            if (command == null) throw new ArgumentNullException(nameof(command));

            var aggType = aggregate.GetType();
            var cmdType = command.GetType();
            var handlers = (from m in aggType.GetRuntimeMethods()
                            let p = m.GetParameters()
                            where p.Length == 1
                            let t = p[0].ParameterType
                            where cmdType.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())
                            select m).ToArray();
            if (handlers.Length < 1) throw new Exception($"There is no handler for \"{cmdType}\" in \"{aggType}\"!");
            if (handlers.Length > 1) throw new Exception($"There is multple handlers for \"{cmdType}\" in \"{aggType}\"!");

            handlers.Single().Invoke(aggregate, new Object[] { command });
            return aggregate.FreshChanges.ToArray();
        }

        #region Internals

        private readonly IMessageBus _bus;
        private readonly Repository _db;
        private readonly Type[] _aggregates;

        private async Task HandleCommand(Command cmd)
        {
            var aggType = GetAggregateTypeForCommand(cmd.GetType());
            var agg = (Aggregate)await _db.Load(cmd.AggregateId, aggType);

            var newEvents = ProduceEvents(agg, cmd);
            if (newEvents.Length > 0) await _db.Save(agg);

            foreach (var e in newEvents) await _bus.Publish(e);
        }

        #endregion
    }
}