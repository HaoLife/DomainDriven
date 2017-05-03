using System;
using System.Linq;
using System.Collections.Generic;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.Repository;
using Rainbow.DomainDriven.Event;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Core.Utilities;
using Rainbow.DomainDriven.RingQueue.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Message;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class EventStoreHandler : IMessageHandler<DomainMessage>
    {
        private readonly IEventSourceRepository _eventSourceRepository;
        private readonly IMessageListening _messageListening;
        private readonly ILogger<EventStoreHandler> _logger;

        public EventStoreHandler(
            IEventSourceRepository eventSourceRepository
            , IMessageListening messageListening
            , ILogger<EventStoreHandler> logger
            )
        {
            this._eventSourceRepository = eventSourceRepository;
            this._messageListening = messageListening;
            this._logger = logger;
        }


        public void Handle(DomainMessage[] messages)
        {
            var data = messages.Where(a => !a.Head.IsSourcing).ToArray();

            var eventSources = data.Select(a => a.Content as DomainEventStream)
                .Where(a => a != null)
                .SelectMany(a => a.EventSources);
            try
            {
                this._eventSourceRepository.AddRange(eventSources);
                this.Notice(data, true);
            }
            catch (Exception ex)
            {
                this.Notice(data, false, ex);
                this._logger.LogCritical(LogEvent.Frame, ex, $"execute name:{nameof(EventStoreHandler)} error");
                throw ex;
            }
        }

        private void Notice(DomainMessage[] data, bool isSuccess, Exception ex = null)
        {
            var message = new NoticeMessage() { IsSuccess = isSuccess, Exception = ex };
            foreach (var item in data)
            {
                if (item.Head.Consistency == ConsistencyLevel.Finally && !string.IsNullOrEmpty(item.Head.ReplyKey))
                    this._messageListening.Notice(item.Head.ReplyKey, message);
            }

        }
    }
}