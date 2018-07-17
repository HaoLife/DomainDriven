using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandMapper
    {

        void Map<TCommand, TAggregateRoot>(Expression<Func<TCommand, Guid>> key)
            where TCommand : ICommand
            where TAggregateRoot : IAggregateRoot;
    }
}
