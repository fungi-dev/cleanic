using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    /// <summary>
    /// Agent behind the bus who handles user commands.
    /// </summary>
    /// <remarks>There can be many logic agent instances for one facade.</remarks>
    public class WriteAgent
    {
        public WriteAgent(ICommandBus cmdBus, IEventBus evtBus, IWriteRepository db, IDomainFacade domain)
        {
            _cmdBus = cmdBus ?? throw new ArgumentNullException(nameof(cmdBus));
            _evtBus = evtBus ?? throw new ArgumentNullException(nameof(evtBus));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _domain = domain;

            _cmdBus.HandleCommands(HandleCommand);
            foreach (var eventType in domain.AffectingEvents)
            {
                _evtBus.ListenEvents(eventType, e => ReactToEvent(e));
            }
        }

        private async Task HandleCommand(ICommand cmd)
        {
            var entity = await _db.Load(cmd.EntityId, cmd.EntityType);
            _domain.ModifyEntity(entity, cmd);
            var events = await _db.Save(entity);
            if (_db != _evtBus)
            {
                foreach (var e in events) await _evtBus.Publish(e);
            }
        }

        private async Task ReactToEvent(IEvent @event)
        {
            var cmds = _domain.ReactToEvent(@event);
            foreach (var cmd in cmds)
            {
                await _cmdBus.Send(cmd);
            }
        }

        private readonly ICommandBus _cmdBus;
        private readonly IEventBus _evtBus;
        private readonly IWriteRepository _db;
        private readonly IDomainFacade _domain;
    }
}