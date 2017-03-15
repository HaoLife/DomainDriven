using System;
using System.Linq;
using System.Collections.Generic;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.Repository;
using Rainbow.DomainDriven.RingQueue.Queue;
using Rainbow.DomainDriven.Event;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Core.Utilities;
using Rainbow.DomainDriven.RingQueue.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Message;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class EventStoreHandler : IQueueHandler<DomainMessage>
    {
        private readonly IEventSourceRepository _eventSourceRepository;
        private readonly IMessageListening _messageListening;
        private readonly ILogger<EventStoreHandler> _logger;
        private List<DomainMessage> _data;
        public EventStoreHandler(
            IEventSourceRepository eventSourceRepository
            , IMessageListening messageListening
            , ILogger<EventStoreHandler> logger
            )
        {
            this._eventSourceRepository = eventSourceRepository;
            this._messageListening = messageListening;
            this._logger = logger;
            this._data = new List<DomainMessage>();
        }

        public void Handle(DomainMessage message, long sequence, bool isEnd)
        {
            //如果是回溯的消息，则不执行保存
            if (message.Head.IsSourcing) return;

            this._data.Add(message);
            if (!isEnd) return;
            if (!this._data.Any()) return;

            var eventSources = this._data.Select(a => a.Content as DomainEventStream)
                .Where(a => a != null)
                .SelectMany(a => a.EventSources);
            try
            {
                this._eventSourceRepository.AddRange(eventSources);
                this.Notice(true);
                this._data.Clear();
            }
            catch (Exception ex)
            {
                this.Notice(false, ex);
                this._logger.LogCritical(LogEvent.Frame, ex, $"execute name:{nameof(EventStoreHandler)} error");
                throw ex;
            }

        }

        private void Notice(bool isSuccess, Exception ex = null)
        {
            var message = new NoticeMessage() { IsSuccess = isSuccess, Exception = ex };
            foreach (var item in this._data)
            {
                if (item.Head.Consistency == ConsistencyLevel.Finally && !string.IsNullOrEmpty(item.Head.ReplyKey))
                    this._messageListening.Notice(item.Head.ReplyKey, message);
            }

        }
    }
}