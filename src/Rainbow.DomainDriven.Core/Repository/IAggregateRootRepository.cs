using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Repository
{
    public interface IAggregateRootRepository<TAggregateRoot> where TAggregateRoot : IAggregateRoot
    {
        void Add(TAggregateRoot root);
        void Update(TAggregateRoot root);
        void Remove(TAggregateRoot root);

        TAggregateRoot Get(Guid id);
        IEnumerable<TAggregateRoot> Get(params Guid[] keys);

        bool Exsits(Guid id);
    }
}