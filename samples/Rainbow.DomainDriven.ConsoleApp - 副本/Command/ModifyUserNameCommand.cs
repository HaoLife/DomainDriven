using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Command;

namespace Rainbow.DomainDriven.ConsoleApp.Command
{
    public class ModifyUserNameCommand : ICommand
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
    }
}
