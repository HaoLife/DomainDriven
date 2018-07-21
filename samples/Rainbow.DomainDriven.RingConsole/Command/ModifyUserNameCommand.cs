using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Command;

namespace Rainbow.DomainDriven.RingConsole.Command
{
    public class ModifyUserNameCommand : AbstractCommand
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
    }
}
