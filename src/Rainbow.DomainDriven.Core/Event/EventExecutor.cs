using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Event
{
    public class EventExecutor : IEventExecutor
    {
        private readonly IEventHandlerProxy _eventHandlerProxy;

        public EventExecutor(IEventHandlerProxy eventHandlerProxy)
        {
            this._eventHandlerProxy = eventHandlerProxy;
        }
        
        public void Handle(DomainMessage<EventStream> message)
        {
            foreach(var item in message.Content.Sources)
            {
                this._eventHandlerProxy.Handle(item.Event);
            }
        }
    }
}