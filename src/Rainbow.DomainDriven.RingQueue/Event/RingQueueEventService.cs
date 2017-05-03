using System;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Utilities;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class RingQueueEventService : IEventService
    {
        private readonly IRingBufferProducer<DomainMessage> _messageProducer;
        
        public RingQueueEventService(
            IMessageProcessBuilder messageProcessBuilder
            )
        {
            var process = messageProcessBuilder.Build();
            var queue = process.GetQueue(QueueName.EventQueue);
            this._messageProducer = new RingBufferProducer<DomainMessage>(queue);
        }

        public void Publish(DomainMessage evt)
        {
            this._messageProducer.Send(evt);
        }
    }
}