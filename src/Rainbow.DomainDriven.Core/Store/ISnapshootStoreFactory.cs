using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Store
{
    public interface ISnapshootStoreFactory
    {
        ISnapshootStore<TAggregateRoot> Create<TAggregateRoot>() where TAggregateRoot : IAggregateRoot;
    }
}
