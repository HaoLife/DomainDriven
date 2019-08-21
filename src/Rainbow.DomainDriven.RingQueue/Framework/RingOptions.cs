using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Framework
{
    public class RingOptions
    {
        public int CommandQueueSize { get; set; }
        public int EventQueueSize { get; set; }
    }
}
