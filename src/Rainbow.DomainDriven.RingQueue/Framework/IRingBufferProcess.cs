using Disruptor;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.RingQueue.Command;
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

        RingBuffer<WrapMessage<CommandMessage>> GetCommand();

        RingBuffer<WrapMessage<IEvent>> GetEvent();
    }
}
