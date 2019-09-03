using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Framework
{
    public class WrapMessage<T>
    {
        public T Value { get; set; }
    }
}
