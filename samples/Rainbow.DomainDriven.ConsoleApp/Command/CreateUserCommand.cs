using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Command;

namespace Rainbow.DomainDriven.ConsoleApp.Command
{
    public class CreateUserCommand : ICommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Sex { get; set; }
    }
}
