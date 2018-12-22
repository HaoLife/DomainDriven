using System;
using System.Collections.Generic;
using System.Text;
using Rainbow.DomainDriven.Event;
using System.Linq;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class EventHandleSubject : IEventHandleSubject
    {
        private List<IEventHandleObserver> _observers = new List<IEventHandleObserver>();

        public void Add(IEventHandleObserver observer)
        {
            _observers.Add(observer);
        }

        public void Remove(Guid subscribeId)
        {
            _observers.RemoveAll(a => a.SubscribeId == subscribeId);
        }

        public void Update(SubscribeEvent subscribeEvent)
        {
            foreach (var item in _observers)
            {
                if (item.SubscribeId == subscribeEvent.Id)
                    item.Update(subscribeEvent);
            }
        }
    }
}
