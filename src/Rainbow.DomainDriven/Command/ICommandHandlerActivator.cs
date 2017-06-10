using System;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandHandlerActivator
    {
        ICommandHandler<TCommand> Create<TCommand>(Type type) where TCommand : ICommand;
    }
}