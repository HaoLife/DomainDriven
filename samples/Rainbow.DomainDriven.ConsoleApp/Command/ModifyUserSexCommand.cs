using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.ConsoleApp.Command
{
    public class ModifyUserSexCommand
    {
        public Guid UserId { get; set; }
        public int Sex { get; set; }
    }
}