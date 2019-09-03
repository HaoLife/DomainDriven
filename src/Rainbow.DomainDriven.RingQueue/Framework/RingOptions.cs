using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Framework
{
    public class RingOptions
    {
        public int CommandQueueSize { get; set; }
        public int EventQueueSize { get; set; }

        public int CommandMaxHandleCount { get; set; } = 1000;
        public int EventMaxHandleCount { get; set; } = 1000;
    }
}
