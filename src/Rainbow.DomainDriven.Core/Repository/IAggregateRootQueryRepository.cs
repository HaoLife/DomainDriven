using System.Linq;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Repository
{
    public interface IAggregateRootQueryRepository<TAggregateRoot> : IQueryable<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot
    {
    }
}