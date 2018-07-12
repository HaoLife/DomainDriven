using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandRegister
    {
        void Add(Type commandType, Type handlerType);

        Type FindHandlerType<TCommand>() where TCommand : ICommand;
    }
}
