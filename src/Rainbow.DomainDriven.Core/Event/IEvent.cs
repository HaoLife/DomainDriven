using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Event
{
    public interface IEvent
    {
        Guid Id { get; set; }
        Guid AggregateRootId { get; }
        string AggregateRootTypeName { get; set; }
        long UTCTimestamp { get; }
        int Version { get; set; }

    }
}
