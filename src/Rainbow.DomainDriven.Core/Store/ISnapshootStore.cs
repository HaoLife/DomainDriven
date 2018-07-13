using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Store
{
    public interface ISnapshootStore<TAggregateRoot> where TAggregateRoot : IAggregateRoot
    {
        void Add(TAggregateRoot aggregate);
        void Add(TAggregateRoot[] aggregates);
        void Update(TAggregateRoot aggregate);
        void Update(TAggregateRoot[] aggregate);
        void Remove(TAggregateRoot aggregate);
        void Remove(TAggregateRoot[] aggregate);
    }
}
