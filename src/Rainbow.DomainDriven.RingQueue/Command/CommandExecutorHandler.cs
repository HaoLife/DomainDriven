using System;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Core.Utilities;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Infrastructure;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class CommandExecutorHandler : IMessageHandler<DomainMessage<ICommand>>
    {
        private readonly ICommandExecutor _commandExecutor;
        public CommandExecutorHandler(
            ICommandExecutor commandExecutor
            )
        {
            this._commandExecutor = commandExecutor;
        }

        public void Handle(DomainMessage<ICommand>[] messages)
        {
            this._commandExecutor.Handle(messages);
        }
    }
}