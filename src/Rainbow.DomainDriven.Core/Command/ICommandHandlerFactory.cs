using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandHandlerFactory
    {
        ICommandHandler<TCommand> Create<TCommand>(Type type) where TCommand : ICommand;

    }
}
