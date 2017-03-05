using System;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class AggregateRootRepositoryProvider : IAggregateRootRepositoryProvider
    {

        private readonly IServiceProvider _serviceProvider;

        public AggregateRootRepositoryProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }


        public IAggregateRootRepository GetRepo(Type aggregateType)
        {
            var genericType = typeof(AggregateRootLockRepository<>).MakeGenericType(aggregateType);
            var repo = this._serviceProvider.GetService(genericType) as IAggregateRootRepository;
            if (repo != null) return repo;
            throw new Exception($"没有找到类型：{aggregateType.Name}");
        }
    }
}