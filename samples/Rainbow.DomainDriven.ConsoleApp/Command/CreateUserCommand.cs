using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.ConsoleApp.Command
{
    public class CreateUserCommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Sex { get; set; }
    }
}
