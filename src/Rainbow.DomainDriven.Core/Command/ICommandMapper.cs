using System;
using System.Linq.Expressions;

namespace Rainbow.DomainDriven.Command
{
    public interface ICommandMapper
    { 
        void Map<TCommand>(Expression<Func<TCommand, Guid>> key, Type type);
        void Validate<TCommand>(Expression<Func<TCommand, Guid>> key, Type type);
    }
}