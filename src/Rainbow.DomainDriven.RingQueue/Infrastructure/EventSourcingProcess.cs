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
        private readonly IRingBufferProducer<DomainMessage<EventStream>> _messageProducer;
        private readonly IEventSourcingRepository _eventSourcingRepository;
        public EventSourcingProcess(
            IMessageProcessBuilder messageProcessBuilder
            , IEventSourcingRepository eventSourcingRepository
            )
        {
            var process = messageProcessBuilder.Build();
            var queue = process.GetQueue<DomainMessage<EventStream>>(QueueName.EventQueue);
            this._messageProducer = new RingBufferProducer<DomainMessage<EventStream>>(queue);
            this._eventSourcingRepository = eventSourcingRepository;
        }
        public void Run()
        {
            var current = _eventSourcingRepository.Current();
            List<EventSource> sourcings = _eventSourcingRepository.Take(current, 1000);

            while (sourcings.Count > 0)
            {
                var messages = sourcings
                    .OrderBy(a => a.Event.UTCTimestamp)
                    .Select(a => new DomainMessage<EventStream>(CreateHead(), CreateStream(a)));
                foreach (var item in messages)
                    _messageProducer.Send(item);

                current = sourcings.Last();
                sourcings = _eventSourcingRepository.Take(current, 1000);
            }
        }

        private MessageHead CreateHead()
        {
            return new MessageHead() { Priority = Priority.Normal, Consistency = Consistency.Lose };
        }
        private EventStream CreateStream(EventSource es)
        {
            return new EventStream() { Sources = new List<EventSource>() { es } };
        }
    }
}