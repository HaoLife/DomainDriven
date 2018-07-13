using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rainbow.DomainDriven.Store
{
    public interface ISnapshootQuery<TAggregateRoot> : IQueryable<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot
    {

    }
}
