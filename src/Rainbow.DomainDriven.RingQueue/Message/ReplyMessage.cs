using System;

namespace Rainbow.DomainDriven.RingQueue.Message
{
    public class ReplyMessage
    {
        
        public Exception Exception { get; set; }
        public bool IsSuccess { get; set; }
        public string ReplyKey { get; set; }
    }
}