using Rainbow.DomainDriven.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandExecutor
    {
        void Handle(params DomainMessage<ICommand>[] messages);
    }
}
