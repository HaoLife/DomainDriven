using System;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandContext
    {
        void Add<TAggregateRoot>(TAggregateRoot aggregate) where TAggregateRoot : class, IAggregateRoot;

        TAggregateRoot Get<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot;
    }
}