using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Command;

namespace Rainbow.DomainDriven.RingConsole.Command
{
    public class CreateUserCommand : AbstractCommand
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public int Sex { get; set; }

        //public List<Guid> ChildUsers { get; set; }
        //public Guid UserId2 { get; set; }
        //public Guid UserId3 { get; set; }
    }
}
