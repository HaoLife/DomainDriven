using Rainbow.DomainDriven.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public interface IEventHandleObserver
    {
        Guid SubscribeId { get; }
        SubscribeEvent SubscribeEvent { get; }

        void Update(SubscribeEvent subscribeEvent);

    }
}
