using System;

namespace Rainbow.DomainDriven.RingQueue.Message
{
    public class NullMessageMonitor : IMessageMonitor
    {
        public void Wait(string key)
        {
        }
    }
}