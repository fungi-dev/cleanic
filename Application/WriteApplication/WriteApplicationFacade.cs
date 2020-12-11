using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class WriteApplicationFacade
    {
        public WriteApplicationFacade(ICommandBus bus, LanguageInfo languageInfo)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            LanguageInfo = languageInfo ?? throw new ArgumentNullException(nameof(languageInfo));
        }

        public async Task Do(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            //todo check if this user allowed to edit this aggregate
            // let admins set permissions for particular aggregates (and maybe for particular aggregate actions)

            //todo serve multitanancy
            //todo inject tech info to command (envelope)

            await _bus.Send(command);
        }

        protected readonly LanguageInfo LanguageInfo;

        private readonly ICommandBus _bus;
    }
}