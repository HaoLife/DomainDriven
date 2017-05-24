using System;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Core.Utilities;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.Repository;
using System.Linq;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class EventExecutorHandler : IMessageHandler<DomainMessage<EventStream>>
    {
        private readonly IEventExecutor _eventExecutor;
        private readonly IEventSourcingRepository _eventSourcingRepository;
        public EventExecutorHandler(
            IEventExecutor eventExecutor
            , IEventSourcingRepository eventSourcingRepository
            )
        {
            this._eventExecutor = eventExecutor;
            this._eventSourcingRepository = eventSourcingRepository;
        }
        
        public void Handle(DomainMessage<EventStream>[] messages)
        {
            foreach (var message in messages)
            {
                try
                {
                    this._eventExecutor.Handle(message);
                }
                catch (Exception ex)
                {

                }

                try
                {
                    this._eventSourcingRepository.Save(message.Content.Sources.Last());
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}