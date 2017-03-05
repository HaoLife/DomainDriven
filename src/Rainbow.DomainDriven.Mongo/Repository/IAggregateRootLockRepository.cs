using System;
using Rainbow.DomainDriven.Domain;
using System.Collections.Generic;
using System.Linq;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public interface IAggregateRootLockRepository : IAggregateRootRepository
    {
        void Lock(IEnumerable<IAggregateRoot> roots, long expires);
        void UnLock(IEnumerable<IAggregateRoot> roots);
    }

}