using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Command;

namespace Rainbow.DomainDriven.RingConsole.Command
{
    public class ModifyUserSexCommand : AbstractCommand
    {
        public Guid UserId { get; set; }
        public int Sex { get; set; }
    }
}