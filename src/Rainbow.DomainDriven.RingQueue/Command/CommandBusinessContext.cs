using Rainbow.DomainDriven.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class CommandBusinessContext
    {
        public ReplyMessage Reply { get; set; }
        public IEnumerable<IEvent> UncommittedEvents { get; set; } = Enumerable.Empty<IEvent>();
        public CommandMessage Message { get; set; }

    }
}
