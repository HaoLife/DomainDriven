using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandContext
    {

        void Add<TAggregateRoot>(TAggregateRoot aggregate) where TAggregateRoot : class, IAggregateRoot;

        TAggregateRoot Get<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot;
    }
}
