using Rainbow.DomainDriven.Store;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;

namespace Rainbow.DomainDriven.Event
{
    public class EventRebuildInitializer : IEventRebuildInitializer
    {

        private volatile int _running;
        private int number = 1000;
        private ISubscribeEventStore _subscribeEventStore;
        private IEventStore _eventStore;
        private IEventBus _eventBus;

        public EventRebuildInitializer(
            ISubscribeEventStore subscribeEventStore
            , IEventStore eventStore
            , IEventBus eventBus)
        {
            _subscribeEventStore = subscribeEventStore;
            _eventStore = eventStore;
            _eventBus = eventBus;
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
            do
            {
                var evts = _eventStore.Take(number, utcTimestamp);
                isUpdate = evts.Count > 0;
                if (!isUpdate) break;
                _eventBus.Publish(evts.ToArray());
                utcTimestamp = evts.Max(a => a.UTCTimestamp);

            } while (isUpdate);

        }
    }
}
