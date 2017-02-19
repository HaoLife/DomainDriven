using System;
using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Event
{
    public class DomainEventSource
    {
        
        public DomainEventSource() { }
        public DomainEventSource(IEvent evt, string aggrTypeName, Guid aggrId)
        {
            this.Event = evt;
            this.AggregateRootTypeName = aggrTypeName;
            this.AggregateRootId = aggrId;
        }
        public string AggregateRootTypeName { get; set; }
        public Guid AggregateRootId { get; set; }
        public IEvent Event { get; set; }
    }
}