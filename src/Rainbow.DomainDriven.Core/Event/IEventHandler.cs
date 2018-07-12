using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventHandler<in TEvent>
        where TEvent : IEvent
    {
        void Handle(TEvent evt);
    }
}
