using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Repository
{
    public interface IAggregateRootRepository
    {
        void Add(IAggregateRoot root);
        void Update(IAggregateRoot root);
        void Remove(IAggregateRoot root);
        void Add(IEnumerable<IAggregateRoot> roots);
        void Update(IEnumerable<IAggregateRoot> roots);
        void Remove(IEnumerable<IAggregateRoot> roots);
        IAggregateRoot Get(Guid id);
        IEnumerable<IAggregateRoot> Get(params Guid[] keys);
    }
}