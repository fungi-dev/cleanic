using Cleanic.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    //todo do logging
    /// <summary>
    /// Agent behind the bus who handles user commands.
    /// </summary>
    /// <remarks>There can be many logic agent instances for one facade.</remarks>
    public class WriteAgent
    {
        public WriteAgent(ICommandBus cmdBus, IEventBus evtBus, IWriteRepository db, DomainMeta domain)
        {
            _cmdBus = cmdBus ?? throw new ArgumentNullException(nameof(cmdBus));
            _evtBus = evtBus ?? throw new ArgumentNullException(nameof(evtBus));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _domain = domain;

            _cmdBus.HandleCommands(HandleCommand);
            foreach (var eventMeta in domain.Services.SelectMany(x => x.Events))
            {
                _evtBus.ListenEvents(eventMeta.Type, e => HandleEvent(e));
            }
        }

        private readonly ICommandBus _cmdBus;
        private readonly IEventBus _evtBus;
        private readonly IWriteRepository _db;
        private readonly DomainMeta _domain;

        private async Task HandleCommand(Command cmd)
        {
            var cmdMeta = _domain.GetCommandMeta(cmd.GetType());
            var aggregate = await _db.LoadOrCreate(cmd.AggregateId, cmdMeta.Aggregate.Type);
            var svcMetas = _domain.GetAggregateMeta(cmdMeta.Aggregate.Type).GetDependencies(cmd);
            await aggregate.Do(cmd, svcMetas.Select(x => x.GetInstance()));
            var events = await _db.Save(aggregate);
            if (_db != _evtBus) foreach (var e in events) await _evtBus.Publish(e);
        }

        private async Task HandleEvent(Event @event)
        {
            foreach (var svcMeta in _domain.Services.Where(s => s.Events.Any(m => m.Type == @event.GetType())))
            {
                var svc = svcMeta.GetInstance();
                var cmds = await svc.Handle(@event);
                foreach (var cmd in cmds) await _cmdBus.Send(cmd);
            }
        }
    }
}