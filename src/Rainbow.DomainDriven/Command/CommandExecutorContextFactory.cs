using System;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Command
{
    public class CommandExecutorContextFactory : ICommandExecutorContextFactory
    {
        private readonly IAggregateRootQuery _aggregateRootQuery;
        public CommandExecutorContextFactory(IAggregateRootQuery aggregateRootQuery)
        {
            this._aggregateRootQuery = aggregateRootQuery;
        }
        public ICommandExecutorContext Create()
        {
            return new CommandExecutorContext(this._aggregateRootQuery);
        }
    }
}