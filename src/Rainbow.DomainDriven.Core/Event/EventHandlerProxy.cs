using System;
using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Event
{
    public class EventHandlerProxy<TEvent> : IEventHandlerProxy
        where TEvent : IEvent
    {

        private readonly IEventHandlerSelector _eventHandlerSelector;
        private readonly IEventHandlerActivator _eventHandlerActivator;

        public EventHandlerProxy(IEventHandlerSelector eventHandlerSelector, IEventHandlerActivator eventHandlerActivator)
        {
            this._eventHandlerSelector = eventHandlerSelector;
            this._eventHandlerActivator = eventHandlerActivator;
        }

        public void HandlerInvoke(Type type, TEvent evt)
        {
            IEventHandler<TEvent> handler;
            try
            {
                handler = this._eventHandlerActivator.Create<TEvent>(type);
            }
            catch (Exception ex)
            {
                throw new Exception("事件执行器创建异常", ex);
            }

            try
            {
                handler.Handle(evt);
            }
            catch (Exception ex)
            {
                throw new Exception("执行事件错误", ex);
            }
        }


        public void Handle(DomainEventSource eventSource)
        {
            var handlerTypes = this._eventHandlerSelector.FindHandlerTypes<TEvent>();

            foreach (var handelType in handlerTypes)
            {
                HandlerInvoke(handelType, (TEvent)eventSource.Event);
            }
        }
    }
}