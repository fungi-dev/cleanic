using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class WriteApplicationFacade
    {
        public WriteApplicationFacade(ICommandBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public async Task Do(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            //todo serve multitanancy
            //todo do authentication
            //todo do authorization
            //todo inject tech info to command (envelope)

            await _bus.Send(command);
        }

        private readonly ICommandBus _bus;
    }
}