using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandHandlerSelector
    {
        Type FindExecutorType<TCommand>() where TCommand : class;
    }
}
