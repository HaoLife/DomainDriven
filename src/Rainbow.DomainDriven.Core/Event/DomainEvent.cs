using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Event
{
    public class DomainEvent : IEvent
    {
        public DomainEvent()
        {
            this.UTCTimestamp = DateTime.Now.ToUniversalTime().Ticks;
        }

        public long UTCTimestamp { get; private set; }

        public int Version { get; set; }

    }
}
