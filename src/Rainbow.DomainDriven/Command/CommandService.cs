using System;
using Rainbow.DomainDriven.Host;
using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Command
{
    public class CommandService : ICommandService
    {
        private readonly IDomainHost _domainHost;
        public CommandService(
            IDomainHost domainHost
            )
        {
            this._domainHost = domainHost;
        }
        public void Publish(ICommand command, MessageDescribe describe)
        {
            var commandExecutor = this._domainHost.Factory.Create(describe);
            commandExecutor.Handle(command);
        }
    }
}