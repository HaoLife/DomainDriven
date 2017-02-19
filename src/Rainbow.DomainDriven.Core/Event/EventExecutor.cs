using System;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Event
{
    public class EventExecutor : IEventExecutor
    {
        private readonly IEventHandlerProxyProvider _eventHandlerProxyProvider;

        public EventExecutor(IEventHandlerProxyProvider eventHandlerProxyProvider)
        {
            this._eventHandlerProxyProvider = eventHandlerProxyProvider;
        }

        public void Handle(DomainMessage<DomainEventStream> message)
        {
            foreach (var item in message.Content.EventSources)
            {
                var proxy = this._eventHandlerProxyProvider.GetEventHanderProxy(item.Event.GetType());
                proxy.Handle(item);
            }
        }
    }
}