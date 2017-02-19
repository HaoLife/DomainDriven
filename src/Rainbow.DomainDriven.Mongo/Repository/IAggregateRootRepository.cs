using System;
using Rainbow.DomainDriven.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public interface IAggregateRootRepository
    {
        void Add(IAggregateRoot root);
        void Update(IAggregateRoot root);
        void Remove(IAggregateRoot root);
        void Lock(RowLock rowLock, IEnumerable<IAggregateRoot> roots);
        void UnLock(IEnumerable<IAggregateRoot> roots);
    }

}