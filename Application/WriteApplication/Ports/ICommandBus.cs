using Cleanic.Core;
using System;
using System.Threading.Tasks;

namespace Cleanic.Application
{
    public interface ICommandBus
    {
        /// <summary>
        /// Send the command with hope that some handler will catch it.
        /// </summary>
        Task Send(Command command);

        /// <summary>
        /// Register the action which will handle all instances of some type of commands.
        /// The only one action can be for each type of command.
        /// </summary>
        void HandleCommands(Func<Command, Task> handler);
    }
}