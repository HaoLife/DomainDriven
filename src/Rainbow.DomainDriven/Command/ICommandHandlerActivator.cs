using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandHandlerActivator
    {
        ICommandHandler<TCommand> Create<TCommand>(Type type) where TCommand : ICommand;
    }
}
