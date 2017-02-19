using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventHandlerActivator
    {
        IEventHandler<TEvent> Create<TEvent>(Type type) where TEvent : IEvent;
    }
}
