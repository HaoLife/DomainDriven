using System;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventHandlerProxyProvider
    {
         IEventHandlerProxy GetEventHandlerProxy(Type eventType);
    }
}