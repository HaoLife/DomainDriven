using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Domain
{
    public interface IAggregateRootRebuilder
    {
        TAggregateRoot Rebuild<TAggregateRoot>(Guid id) where TAggregateRoot : IAggregateRoot;

        IAggregateRoot Rebuild(Type type, Guid id);
        IAggregateRoot Rebuild(IAggregateRoot aggregateRoot);
        IEnumerable<IAggregateRoot> Rebuild(Type type, Guid[] ids);

    }

}
