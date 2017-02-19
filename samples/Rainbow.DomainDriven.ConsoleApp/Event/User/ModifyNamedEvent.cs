using Rainbow.DomainDriven.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.ConsoleApp.Event.User
{
    public class ModifyNamedEvent : DomainEvent
    {
        public string Name { get; set; }
    }
}
