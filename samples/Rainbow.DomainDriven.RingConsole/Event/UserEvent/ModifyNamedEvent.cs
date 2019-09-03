using Rainbow.DomainDriven.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.RingConsole.Event.UserEvent
{
    public class ModifyNamedEvent : DomainEvent
    {
        public string Name { get; set; }


        public override string ToString()
        {
            return $"{this.AggregateRootId}- {this.Name}";
        }
    }
}
