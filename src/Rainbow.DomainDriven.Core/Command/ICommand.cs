using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommand
    {
        Guid Id { get;}
        int Priority { get; }
    }
}
