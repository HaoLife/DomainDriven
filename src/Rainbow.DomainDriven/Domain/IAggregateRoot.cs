using Rainbow.DomainDriven.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Domain
{
    public interface IAggregateRoot : IEntity
    {
        int Version { get; }
        IEnumerable<IEvent> UncommittedEvents { get; }
        void Clear();
        void ReplayEvent<TEvent>(TEvent evt) where TEvent : IEvent;
    }
}
