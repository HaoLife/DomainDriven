using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Rainbow.DomainDriven.Domain;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Rainbow.DomainDriven.Repository;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class AggregateRootLockRepositoryProvider : IAggregateRootLockRepositoryProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public AggregateRootLockRepositoryProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }


        public IAggregateRootLockRepository GetRepo(Type aggregateType)
        {
            var genericType = typeof(AggregateRootLockRepository<>).MakeGenericType(aggregateType);
            var repo = this._serviceProvider.GetService(genericType) as IAggregateRootLockRepository;
            if (repo != null) return repo;
            throw new Exception($"没有找到类型：{aggregateType.Name}");
        }
    }
}