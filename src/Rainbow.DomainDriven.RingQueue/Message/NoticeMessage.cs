using System;

namespace Rainbow.DomainDriven.RingQueue.Message
{
    public class NoticeMessage
    {
        public bool IsSuccess { get; set; }
        public Exception Exception { get; set; }
    }
}