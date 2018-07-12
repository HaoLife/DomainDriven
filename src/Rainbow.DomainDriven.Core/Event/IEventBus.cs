using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventBus
    {
        void Publish(IEvent[] events);
    }
}
