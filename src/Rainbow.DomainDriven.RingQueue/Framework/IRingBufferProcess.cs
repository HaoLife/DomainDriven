using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.RingQueue.Command;
using Rainbow.MessageQueue.Ring;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Framework
{
    public interface IRingBufferProcess
    {
        bool IsStart { get; }

        void Start();

        void Stop();

        RingBuffer<CommandMessage> GetCommand();

        RingBuffer<IEvent> GetEvent();
    }
}
