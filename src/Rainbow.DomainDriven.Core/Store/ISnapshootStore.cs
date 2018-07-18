using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Store
{
    public interface ISnapshootStore
    {
        void Add(IAggregateRoot aggregate);
        void Add(IAggregateRoot[] aggregates);
        void Update(IAggregateRoot aggregate);
        void Update(IAggregateRoot[] aggregates);
        void Remove(IAggregateRoot aggregate);
        void Remove(IAggregateRoot[] aggregates);

        IAggregateRoot Get(Guid id);
        List<IAggregateRoot> Get(Guid[] ids);
    }
}
