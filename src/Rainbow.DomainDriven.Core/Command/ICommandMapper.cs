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

        void MapList<TCommand, TAggregateRoot>(Expression<Func<TCommand, IEnumerable<Guid>>> key)
            where TCommand : ICommand
            where TAggregateRoot : IAggregateRoot;

        void Unique<TCommand>(Expression<Func<TCommand, bool>> veri);
    }
}
