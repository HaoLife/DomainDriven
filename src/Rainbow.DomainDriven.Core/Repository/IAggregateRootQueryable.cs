using System.Linq;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Repository
{
    public interface IAggregateRootQueryable<TAggregateRoot> : IQueryable<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot
    {
    }
}