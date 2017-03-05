using System;
using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Domain
{
    public class ReplayEventProxy<TEvent>
        : IReplayEventProxy
        where TEvent : class, IEvent
    {
        public void Handle(IAggregateRoot root, IEvent evt)
        {
            root.ReplayEvent<TEvent>(evt as TEvent);
        }
    }
}