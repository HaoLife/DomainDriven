using System;
using System.Linq.Expressions;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandMapper
    {

        void Map<TCommand, TAggregateRoot>(Expression<Func<TCommand, Guid>> key)
            where TCommand : ICommand
            where TAggregateRoot : IAggregateRoot;
    }
}