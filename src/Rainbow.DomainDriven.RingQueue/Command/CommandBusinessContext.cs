using Rainbow.DomainDriven.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class CommandBusinessContext
    {
        public ReplyMessage Reply { get; set; }
        public IEnumerable<IEvent> UncommittedEvents { get; set; }

        public void Clear()
        {
            this.Reply = null;
            this.UncommittedEvents = null;
        }
    }
}
