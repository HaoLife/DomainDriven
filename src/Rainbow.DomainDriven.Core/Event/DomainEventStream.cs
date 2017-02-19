using System.Collections.Generic;

namespace Rainbow.DomainDriven.Event
{
    public class DomainEventStream
    {
        public List<DomainEventSource> EventSources { get; set; }
    }
}