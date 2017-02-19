using System;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventHandlerProxyProvider
    {
         IEventHandlerProxy GetEventHanderProxy(Type eventType);
    }
}