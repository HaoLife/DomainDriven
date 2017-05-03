using System;
using System.Collections.Generic;
using System.Linq;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.Repository;
using Rainbow.DomainDriven.RingQueue.Utilities;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public class EventSourcingProcess : IEventSourcingProcess
    {
        private readonly IRingBufferProducer<DomainMessage> _messageProducer;
        private readonly IEventSourcingRepository _eventSourcingRepository;
        public EventSourcingProcess(
            IMessageProcessBuilder messageProcessBuilder
            , IEventSourcingRepository eventSourcingRepository
            )
        {
            var process = messageProcessBuilder.Build();
            var queue = process.GetQueue(QueueName.EventQueue);
            this._messageProducer = new RingBufferProducer<DomainMessage>(queue);
            this._eventSourcingRepository = eventSourcingRepository;
        }
        public void Run()
        {
            var current = _eventSourcingRepository.Current();
            List<DomainEventSource> sourcings = _eventSourcingRepository.Take(current, 1000);

            while (sourcings.Count > 0)
            {
                var messages = sourcings
                    .OrderBy(a => a.Event.UTCTimestamp)
                    .Select(a => new DomainMessage(CreateHead(), CreateStream(a)));
                foreach (var item in messages)
                    _messageProducer.Send(item);

                current = sourcings.Last();
                sourcings = _eventSourcingRepository.Take(current, 1000);
            }
        }

        private MessageHead CreateHead()
        {
            return new MessageHead(Guid.NewGuid().ToShort(), true);
        }
        private DomainEventStream CreateStream(DomainEventSource es)
        {
            return new DomainEventStream() { EventSources = new List<DomainEventSource>() { es } };
        }
    }
}