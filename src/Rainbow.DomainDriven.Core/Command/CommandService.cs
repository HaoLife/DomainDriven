using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Command
{
    //职责:
    //1.根据事件级别进行分别处理
    //

    public class CommandService : ICommandService
    {
        private readonly ICommandExecutor _commandExecutor;

        public CommandService(
            ICommandExecutor commandExecutor
            )
        {
            this._commandExecutor = commandExecutor;
        }


        public void Publish<TCommand>(DomainMessage<TCommand> cmd) where TCommand : class
        {
            if (string.IsNullOrEmpty(cmd.Head.ReplyKey))
            {
                Task.Factory.StartNew((message) => _commandExecutor.Handle(message as DomainMessage<TCommand>), cmd);
            }
            else
            {
                _commandExecutor.Handle(cmd);
            }
        }

    }
}
