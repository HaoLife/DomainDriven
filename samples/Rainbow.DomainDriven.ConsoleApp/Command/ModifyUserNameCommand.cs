using Rainbow.DomainDriven.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.ConsoleApp.Command
{
    public class ModifyUserNameCommand
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
    }
}
