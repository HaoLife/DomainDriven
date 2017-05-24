using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Command;

namespace Rainbow.DomainDriven.ConsoleApp.Command
{
    public class ModifyUserSexCommand : ICommand
    {
        public Guid UserId { get; set; }
        public int Sex { get; set; }
    }
}