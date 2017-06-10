using System;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandHandlerSelector
    {
        Type FindHandlerType<TCommand>() where TCommand : ICommand;
    }
}