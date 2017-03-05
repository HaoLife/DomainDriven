using System;
using System.Collections.Generic;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Repository
{
    public interface IAggregateRootBatchRepository
    {
         
        void Add(IAggregateRoot root);
        void Add(IEnumerable<IAggregateRoot> roots);
        void Update(IAggregateRoot root);
        void Update(IEnumerable<IAggregateRoot> roots);
        void Remove(IAggregateRoot root);
        void Remove(IEnumerable<IAggregateRoot> roots);

        IAggregateRoot Get(Guid id);
        IEnumerable<IAggregateRoot> Get(params Guid[] keys);
        void Commit();
    }
}