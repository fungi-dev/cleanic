using Cleanic.Domain;
using Cleanic.DomainInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    /// <summary>
    /// Agent behind the bus who handles user commands.
    /// </summary>
    /// <remarks>There can be many logic agent instances for one facade.</remarks>
    public class LogicAgent
    {
        public LogicAgent(IMessageBus bus, IRepository db, DomainInfo.DomainInfo domain, Func<Type, IService[]> domainServiceFactory)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _aggregates = domain.Aggregates.Select(x => x.Type).ToArray();
            _services = domainServiceFactory;

            bus.HandleCommands(HandleCommand);
            foreach (var s in domain.Sagas)
            {
                foreach (var eventType in s.TriggerEventTypes)
                {
                    bus.ListenEvents(eventType, e => RunSaga(s, e));
                }
            }
        }

        private async Task HandleCommand(Command cmd)
        {
            var aggType = FindAggregate(cmd.GetType());
            var agg = (IAggregate)await _db.Load(cmd.SubjectId, aggType);
            var handler = FindCommandHandler(agg, cmd);

            var result = await handler.Invoke();
            result.SubjectId = cmd.Id;
            var newEvents = agg.FreshChanges.ToArray();
            if (newEvents.Length > 0) await _db.Save(agg);

            foreach (var e in newEvents) await _bus.Publish(e);
            foreach (var c in agg.FreshCommands) await _bus.Send(c);
            await _bus.Publish(result);
        }

        private async Task RunSaga(SagaInfo sagaInfo, Event e)
        {
            var aggType = FindAggregate(e.GetType());
            var eventType = e.GetType();
            var saga = (ISaga)Activator.CreateInstance(sagaInfo.Type);
            var agg = (IAggregate)await _db.Load(e.SubjectId, aggType);
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

        protected virtual Func<Task<Command.Result>> FindCommandHandler(IAggregate aggregate, Command command)
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

            var servicesForHandler = handler.GetParameters().Skip(1).Select(x => _services(x.ParameterType));
            var handlerParams = new List<Object> { command };
            handlerParams.AddRange(servicesForHandler);
            return () => (Task<Command.Result>)handler.Invoke(aggregate, handlerParams.ToArray());
        }

        protected virtual Func<Task<Command[]>> FindEventHandler(ISaga saga, Event e, IAggregate agg)
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

            var handlerParams = new List<Object> { e };
            if (handler.GetParameters().Count() > 1) handlerParams.Add(agg);
            return () => (Task<Command[]>)handler.Invoke(saga, handlerParams.ToArray());
        }

        private readonly IMessageBus _bus;
        private readonly IRepository _db;
        private readonly Type[] _aggregates;
        private readonly Func<Type, IService[]> _services;
    }
}