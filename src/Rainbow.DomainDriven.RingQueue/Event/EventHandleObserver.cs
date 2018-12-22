using System;
using System.Collections.Generic;
using System.Text;
using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class EventHandleObserver : IEventHandleObserver
    {
        private SubscribeEvent _subscribeEvent;

        public EventHandleObserver(Guid subscribeId)
        {
            this._subscribeEvent = new SubscribeEvent() { Id = subscribeId, UTCTimestamp = 0 };
        }

        public Guid SubscribeId => _subscribeEvent.Id;

        public SubscribeEvent SubscribeEvent => _subscribeEvent;


        public void Update(SubscribeEvent subscribeEvent)
        {
            _subscribeEvent.AggregateRootId = subscribeEvent.AggregateRootId;
            _subscribeEvent.AggregateRootTypeName = subscribeEvent.AggregateRootTypeName;
            _subscribeEvent.EventId = subscribeEvent.EventId;
            _subscribeEvent.UTCTimestamp = subscribeEvent.UTCTimestamp;
        }
    }
}
