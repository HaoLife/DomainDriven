using System.Collections.Generic;

namespace Rainbow.DomainDriven.Event
{
    public class EventStream
    {
        public EventStream() { }
        public EventStream(List<EventSource> sources)
        {
            this.Sources = sources;
        }
        public List<EventSource> Sources { get; set; }
    }
}