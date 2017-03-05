using System;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class AggregateRootBatchRepositoryProvider : IAggregateRootBatchRepositoryProvider
    {
        private readonly IServiceProvider _serviceProvider;
        public AggregateRootBatchRepositoryProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public IAggregateRootBatchRepository GetRepo(Type aggregateType)
        {
            var genericType = typeof(AggregateRootBatchRepository<>).MakeGenericType(aggregateType);
            var proxy = this._serviceProvider.GetService(genericType);
            if (proxy != null) return proxy as IAggregateRootBatchRepository;

            throw new NullReferenceException($"没有找到类型：{aggregateType.Name}");
        }
    }
}