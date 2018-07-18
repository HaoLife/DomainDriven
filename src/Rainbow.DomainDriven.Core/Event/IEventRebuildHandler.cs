using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Event
{
    public interface IEventRebuildHandler
    {
        void Handle(IAggregateRoot root, IEvent evt);
    }
}
