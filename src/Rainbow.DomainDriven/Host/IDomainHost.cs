using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Command;

namespace Rainbow.DomainDriven.Host
{
    public interface IDomainHost
    {
        ICommandExecutorFactory Factory { get; }
        void Start();
    }
}
