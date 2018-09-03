using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class ReplyMessage
    {
        public ReplyMessage(Guid commandId, Exception exception)
            : this(commandId, false, exception)
        {
        }
        public ReplyMessage(Guid commandId)
            : this(commandId, true, null)
        {

        }
        internal ReplyMessage(Guid commandId, bool isSuccess, Exception exception)
        {
            this.CommandId = commandId;
            this.IsSuccess = isSuccess;
            this.Exception = exception;
        }

        public Guid CommandId { get; private set; }
        public Exception Exception { get; set; }
        public bool IsSuccess { get; set; }
        public long Seq { get; set; }
    }
}
