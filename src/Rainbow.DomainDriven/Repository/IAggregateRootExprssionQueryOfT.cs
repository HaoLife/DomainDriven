using System;
using System.Collections.Generic;
using System.Linq;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Repository
{
    public interface IAggregateRootExprssionQueryOfT<TAggregateRoot> : IQueryable<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot
    {
    }
}