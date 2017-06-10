using System;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Core.Utilities;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.Repository;
using System.Linq;
using Rainbow.MessageQueue.Ring;
using Rainbow.DomainDriven.RingQueue.Message;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class MessageEventExecutorHandler : IMessageHandler<DomainMessage<EventStream>>
    {
        private readonly IEventExecutor _eventExecutor;
        private readonly IEventSourcingRepository _eventSourcingRepository;
        private readonly ILogger _logger;
        public MessageEventExecutorHandler(
            IEventExecutor eventExecutor
            , IEventSourcingRepository eventSourcingRepository
            , ILoggerFactory loggerFactory
            )
        {
            this._eventExecutor = eventExecutor;
            this._eventSourcingRepository = eventSourcingRepository;
            this._logger = loggerFactory.CreateLogger<MessageEventExecutorHandler>();
        }

        public void Handle(DomainMessage<EventStream>[] messages)
        {
            foreach (var message in messages)
            {
                try
                {
                    this._eventExecutor.Handle(message.Content.Sources.ToArray());
                }
                catch (Exception ex)
                {
                    this._logger.LogError(1, ex, "执行事件失败");
                }
            }

            try
            {
                this._eventSourcingRepository.Save(messages.Last().Content.Sources.Last());
            }
            catch (Exception ex)
            {
                this._logger.LogError(1, ex, "存储当前已执行的事件源失败");
            }
        }
    }
}