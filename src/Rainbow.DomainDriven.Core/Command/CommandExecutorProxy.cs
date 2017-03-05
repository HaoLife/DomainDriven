using System;
using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Command
{
    public class CommandExecutorProxy<TCommand>
        : ICommandExecutorProxy
        where TCommand : class
    {
        private readonly ICommandExecutor _commandExecutor;
        public CommandExecutorProxy(ICommandExecutor commandExecutor)
        {
            this._commandExecutor = commandExecutor;
        }
        public void Handle(DomainMessage domainMessage)
        {
            this._commandExecutor.Handle<TCommand>(domainMessage);
        }
    }
}