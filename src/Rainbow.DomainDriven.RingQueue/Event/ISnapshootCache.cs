using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public interface ISnapshootCache
    {

        IAggregateRoot Get(Type aggregateType, Guid id);
        IAggregateRoot Set(IAggregateRoot aggregate);
        IAggregateRoot Set(Type type, Guid id, IAggregateRoot aggregate);
        IAggregateRoot Remove(IAggregateRoot aggregate);
        bool Exists(Type aggregateType, Guid id);
        bool Exists(IAggregateRoot aggregate);
    }
}
