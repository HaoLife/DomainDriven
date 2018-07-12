using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Domain
{
    public interface IAggregateRootRebuilder
    {
        IAggregateRoot Get<TAggregateRoot>(Guid id) where TAggregateRoot : IAggregateRoot;

    }

}
