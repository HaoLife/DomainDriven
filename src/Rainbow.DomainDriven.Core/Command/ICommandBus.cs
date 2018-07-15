using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandBus
    {
        Task Publish(ICommand command);
    }
}
