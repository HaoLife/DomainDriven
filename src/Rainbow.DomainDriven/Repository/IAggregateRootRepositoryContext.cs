using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Repository
{
    public interface IAggregateRootRepositoryContext
    {
        void Add(IAggregateRoot aggregate);
        void Update(IAggregateRoot aggregate);
        void Remove(IAggregateRoot aggregate);
        void Commit();
        void RollBack();
    }
}
