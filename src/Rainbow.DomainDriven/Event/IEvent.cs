using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Event
{
    public interface IEvent
    {
        long UTCTimestamp { get; }
        int Version { get; set; }
    }
}
