using Rainbow.DomainDriven.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public interface IEventHandleSubject
    {
        void Add(IEventHandleObserver observer);
        void Remove(Guid subscribeId);

        void Update(SubscribeEvent subscribeEvent);
    }
}
