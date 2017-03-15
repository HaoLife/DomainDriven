using System;
using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Mongo
{
    public class DomainEventSourcing
    {
        public Guid Id { get; set; }
        public DomainEventSource EventSrouce { get; set; }
    }
}