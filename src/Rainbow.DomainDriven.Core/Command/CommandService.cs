using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Command
{
    public class CommandService : ICommandService
    {
        private readonly ICommandExecutor _commandExecutor;

        public CommandService(
            ICommandExecutor commandExecutor
            )
        {
            this._commandExecutor = commandExecutor;
        }

        public void Publish(DomainMessage<ICommand> message)
        {
            switch (message.Head.Consistency)
            {
                case Consistency.Lose:
                    Task.Factory.StartNew(item => this._commandExecutor.Handle(item as DomainMessage<ICommand>), message);
                    break;
                case Consistency.Finally:
                case Consistency.Strong:
                    this._commandExecutor.Handle(message);
                    break;
            }
        }

    }
}
