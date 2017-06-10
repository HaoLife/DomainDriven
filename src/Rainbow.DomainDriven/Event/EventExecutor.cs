using System;

namespace Rainbow.DomainDriven.Event
{
    public class EventExecutor : IEventExecutor
    {

        private readonly IEventHandler _eventHandler;

        public EventExecutor(IEventHandler eventHandler)
        {
            this._eventHandler = eventHandler;
        }
        
        public void Handle(params EventSource[] sources)
        {
            foreach (var item in sources)
            {
                this._eventHandler.Handle(item.Event);
            }
        }
    }
}