using Rainbow.DomainDriven.Store;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Rainbow.DomainDriven.Event
{
    public class EventRebuildInitializer : IEventRebuildInitializer
    {

        private volatile int _running;
        private int number = 1000;
        private ISubscribeEventStore _subscribeEventStore;
        private IEventStore _eventStore;
        private IEventBus _eventBus;
        private ILogger _logger;

        public EventRebuildInitializer(
            ISubscribeEventStore subscribeEventStore
            , IEventStore eventStore
            , IEventBus eventBus
            , ILoggerFactory loggerFactory)
        {
            _subscribeEventStore = subscribeEventStore;
            _eventStore = eventStore;
            _eventBus = eventBus;
            _logger = loggerFactory.CreateLogger<EventRebuildInitializer>();
        }

        public void Initialize()
        {
            if (Interlocked.Exchange(ref _running, 1) != 0)
            {
                throw new InvalidOperationException("Thread is already running");
            }
            var subEvents = _subscribeEventStore.Get();
            long utcTimestamp = 0;
            if (subEvents.Count() > 0)
            {
                utcTimestamp = subEvents.Min(a => a.UTCTimestamp);
            }

            bool isUpdate = true;
            _logger.LogInformation("系统正在重建事件中");
            do
            {
                var evts = _eventStore.Take(number, utcTimestamp);
                _logger.LogInformation($"重建事件 [{evts.Count}] 条");
                isUpdate = evts.Count > 0;
                if (!isUpdate) break;
                _eventBus.Publish(evts.ToArray());
                utcTimestamp = evts.Max(a => a.UTCTimestamp);

            } while (isUpdate);
            _logger.LogInformation($"完成事件重建");

        }
    }
}
