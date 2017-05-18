using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventHandler<in TEvent> 
        where TEvent : IEvent
    {
        void Handle(TEvent evt);
    }
}
