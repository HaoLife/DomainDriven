using Rainbow.DomainDriven.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.ConsoleApp.Event.UserEvent
{
    public class CreatedEvent : DomainEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Sex { get; set; }
    }
}
