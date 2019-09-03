using Rainbow.DomainDriven.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.RingConsole.Event.UserEvent
{
    public class CreatedEvent : DomainEvent
    {
        public CreatedEvent()
            : base(EventOperation.Created)
        {

        }
        public string Name { get; set; }
        public int Sex { get; set; }


        public override string ToString()
        {
            return $"{this.AggregateRootId}- {this.Name}";
        }
    }
}
