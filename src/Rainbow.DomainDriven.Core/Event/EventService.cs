using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Event
{
    public class EventService : IEventService
    {
        private readonly IEventExecutor _eventExecutor;
        public EventService(IEventExecutor eventExecutor)
        {
            this._eventExecutor = eventExecutor;
        }

        public void Publish(DomainMessage evt)
        {
            if (string.IsNullOrEmpty(evt.Head.ReplyKey))
            {
                Task.Factory.StartNew((message) => _eventExecutor.Handle(message as DomainMessage), evt);
            }
            else
            {
                _eventExecutor.Handle(evt);
            }

        }
    }
}
