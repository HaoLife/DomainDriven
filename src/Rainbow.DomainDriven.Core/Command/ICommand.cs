using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommand
    {
        Guid Id { get; }
        PriorityLevel Priority { get; }

        WaitLevel Wait { get; }
    }
}
