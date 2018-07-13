using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Store
{
    public interface ISnapshootQueryFactory
    {
        ISnapshootQuery<TAggregateRoot> Create<TAggregateRoot>() where TAggregateRoot : IAggregateRoot;
    }
}
