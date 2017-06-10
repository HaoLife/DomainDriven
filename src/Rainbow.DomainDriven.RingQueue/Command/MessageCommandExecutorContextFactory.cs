using Rainbow.DomainDriven.Cache;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class MessageCommandExecutorContextFactory : ICommandExecutorContextFactory
    {
        private readonly IAggregateRootQuery _aggregateRootQuery;
        private readonly IAggregateRootCache _aggregateRootCache;
        public MessageCommandExecutorContextFactory(
            IAggregateRootQuery aggregateRootQuery
            , IAggregateRootCache aggregateRootCache)
        {
            this._aggregateRootQuery = aggregateRootQuery;
            this._aggregateRootCache = aggregateRootCache;
        }
        public ICommandExecutorContext Create()
        {
            return new MessageCommandExecutorContext(this._aggregateRootQuery, this._aggregateRootCache);
        }
    }
}