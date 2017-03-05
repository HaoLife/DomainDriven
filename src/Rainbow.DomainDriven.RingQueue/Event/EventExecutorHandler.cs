using System;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Core.Utilities;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Queue;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class EventExecutorHandler : IQueueHandler<DomainMessage>
    {
        private readonly IEventExecutor _eventExecutor;
        private readonly ILogger<EventExecutorHandler> _logger;
        public EventExecutorHandler(
            IEventExecutor eventExecutor
            , ILogger<EventExecutorHandler> logger
            )
        {
            this._eventExecutor = eventExecutor;
            this._logger = logger;
        }
        public void Handle(DomainMessage message, long sequence, bool isEnd)
        {
            try
            {
                this._eventExecutor.Handle(message);
            }
            catch (Exception ex)
            {
                this._logger.LogCritical(LogEvent.Frame, ex, "未知异常：该异常不应该被捕获，请注意该问题");
            }
        }

    }
}