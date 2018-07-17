using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Event
{
    public class SubscribeEvent
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid AggregateRootId { get; set; }
        public string AggregateRootTypeName { get; set; }
        public long UTCTimestamp { get; set; }
    }
}
