using FrogsTalks.Application.Ports;
using FrogsTalks.Domain;
using FrogsTalks.DomainInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
        /// <param name="domainInfo">Domain which logic should be applied.</param>
        public LogicAgent(IMessageBus bus, Repository db, DomainInfo.DomainInfo domainInfo, Func<Type, IDomainService> domainServiceFactory)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _aggregates = domainInfo.Aggregates.Select(x => x.Type).ToArray();
            _factory = domainServiceFactory;

            bus.HandleCommands(HandleCommand);
            foreach (var s in domainInfo.Sagas)
            {
                foreach (var eventType in s.TriggerEventTypes)
                {
                    bus.ListenEvent(eventType, e => RunSaga(s, e));
                }
            }
        }

        private async Task HandleCommand(Command cmd)
        {
            var aggType = FindAggregate(cmd.GetType());
            var agg = (Aggregate)await _db.Load(cmd.AggregateId, aggType);
            var handler = FindCommandHandler(agg, cmd);

            await handler.Invoke();
            var newEvents = agg.FreshChanges.ToArray();
            if (newEvents.Length > 0) await _db.Save(agg);

            foreach (var e in newEvents) await _bus.Publish(e);
        }

        private async Task RunSaga(SagaInfo sagaInfo, Event e)
        {
            var aggType = FindAggregate(e.GetType());
            var eventType = e.GetType();
            var saga = (Saga)Activator.CreateInstance(sagaInfo.Type);
            var agg = (Aggregate)await _db.Load(e.AggregateId, aggType);
            var handler = FindEventHandler(saga, e, agg);
            var cmds = await handler.Invoke();
            foreach (var c in cmds) await _bus.Send(c);
        }

        protected virtual Type FindAggregate(Type commandOrEventType)
        {
            foreach (var aggregate in _aggregates)
            {
                var nested = aggregate.GetTypeInfo().DeclaredNestedTypes;
                if (nested.Contains(commandOrEventType.GetTypeInfo())) return aggregate;
            }
            throw new Exception($"There is no aggregate for \"{commandOrEventType}\" in the domain!");
        }

        protected virtual Func<Task> FindCommandHandler(Aggregate aggregate, Command command)
        {
            var aggType = aggregate.GetType();
            var cmdType = command.GetType();
            var handlers = (from m in aggType.GetRuntimeMethods()
                            let p = m.GetParameters()
                            where p.Length >= 1
                            let t = p[0].ParameterType
                            where cmdType.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())
                            select m).ToArray();
            if (handlers.Length < 1) throw new Exception($"There is no handler for \"{cmdType}\" in \"{aggType}\"!");
            if (handlers.Length > 1) throw new Exception($"There is multple handlers for \"{cmdType}\" in \"{aggType}\"!");
            var handler = handlers.Single();

            var servicesForHandler = handler.GetParameters().Skip(1).Select(x => _factory(x.ParameterType));
            var handlerParams = new List<Object> { command };
            handlerParams.AddRange(servicesForHandler);
            return () => (Task)handler.Invoke(aggregate, handlerParams.ToArray());
        }

        protected virtual Func<Task<Command[]>> FindEventHandler(Saga saga, Event e, Aggregate agg)
        {
            var sagaType = saga.GetType();
            var eType = e.GetType();
            var handlers = (from m in sagaType.GetRuntimeMethods()
                            let p = m.GetParameters()
                            where p.Length >= 1
                            let t = p[0].ParameterType
                            where eType.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())
                            select m).ToArray();
            if (handlers.Length < 1) throw new Exception($"There is no handler for \"{eType}\" in \"{sagaType}\"!");
            if (handlers.Length > 1) throw new Exception($"There is multple handlers for \"{eType}\" in \"{sagaType}\"!");
            var handler = handlers.Single();

            var aggregateForHandler = handler.GetParameters().Skip(1).Select(x => _factory(x.ParameterType));

            var handlerParams = new List<Object> { e };
            if (handler.GetParameters().Count() > 1) handlerParams.Add(agg);
            return () => (Task<Command[]>)handler.Invoke(saga, handlerParams.ToArray());
        }

        private readonly IMessageBus _bus;
        private readonly Repository _db;
        private readonly Type[] _aggregates;
        private readonly Func<Type, IDomainService> _factory;
    }
}