using System;

namespace Rainbow.DomainDriven.Message
{
    public class ReplyMessage
    {
        public Exception Exception { get; set; }
        public bool IsSuccess { get; set; }
    }
}