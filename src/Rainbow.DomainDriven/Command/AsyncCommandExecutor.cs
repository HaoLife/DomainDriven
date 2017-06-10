using System;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Command
{
    public class AsyncCommandExecutor : ICommandExecutor
    {
        private readonly CommandExecutor _commandExecutor;
        public AsyncCommandExecutor(CommandExecutor commandExecutor)
        {
            this._commandExecutor = commandExecutor;
        }
        public void Handle(ICommand command)
        {
            Task.Factory.StartNew((_) => _commandExecutor.Handle(_ as ICommand), command);
        }
    }
}