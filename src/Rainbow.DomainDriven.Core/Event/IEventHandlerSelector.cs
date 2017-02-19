using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventHandlerSelector
    {
        IEnumerable<Type> FindHandlerTypes<TEvent>() where TEvent : IEvent;

    }
}
