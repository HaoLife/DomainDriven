using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventRegister
    {
        void Add(Type eventType, Type handlerType);

        IEnumerable<Type> FindHandlerType<TEvent>() where TEvent : IEvent;
    }
}
