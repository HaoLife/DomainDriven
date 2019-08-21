using Rainbow.DomainDriven.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class CommandMessage
    {
        public CommandMessage(ICommand cmd)
        {
            this.Cmd = cmd;
            this.Notice = new AutoResetEvent(false);
        }

        public ICommand Cmd { get; private set; }
        public AutoResetEvent Notice { get; private set; }
        public ReplyMessage Reply { get; set; }
    }
}
