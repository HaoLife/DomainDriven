using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventHandlerFactory
    {
        IEventHandler<TEvent> Create<TEvent>(Type type) where TEvent : IEvent;
    }
}
