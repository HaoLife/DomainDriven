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
        void Add(IEnumerable<IAggregateRoot> aggregates);
        void Update(IAggregateRoot aggregate);
        void Update(IEnumerable<IAggregateRoot> aggregates);
        void Remove(IAggregateRoot aggregate);
        void Remove(IEnumerable<IAggregateRoot> aggregates);
        void Commit();
        void RollBack();
    }
}
