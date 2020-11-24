using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public class WriteApplicationFacade
    {
        public WriteApplicationFacade(ICommandBus bus, Authorization authorization, LanguageInfo languageInfo)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            LanguageInfo = languageInfo ?? throw new ArgumentNullException(nameof(languageInfo));
        }

        public async Task Do(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var commandInfo = LanguageInfo.GetCommand(command.GetType());
            if (!_authorization.IsAllowed(command.UserId, commandInfo, command.AggregateId)) throw new Exception("Unauthorized");

            //todo serve multitanancy
            //todo inject tech info to command (envelope)

            await _bus.Send(command);
        }

        protected readonly LanguageInfo LanguageInfo;

        private readonly ICommandBus _bus;
        private readonly Authorization _authorization;
    }
}