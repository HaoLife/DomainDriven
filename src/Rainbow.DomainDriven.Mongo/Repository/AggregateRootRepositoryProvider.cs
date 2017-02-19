using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Rainbow.DomainDriven.Domain;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class AggregateRootRepositoryProvider : IAggregateRootRepositoryProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceCollection _serviceCollection;

        private readonly ConcurrentDictionary<Type, IAggregateRootRepository> _cacheAggregateRootRepo =
               new ConcurrentDictionary<Type, IAggregateRootRepository>();

        public AggregateRootRepositoryProvider(IServiceCollection serviceCollection, IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            this._serviceCollection = serviceCollection;
        }

        public void Initialize(IEnumerable<Assembly> assemblys)
        {
            this.RegisterExecutor(assemblys.SelectMany(p => p.GetTypes()));
        }

        private void RegisterExecutor(Type type)
        {
            var info = type.GetTypeInfo();
            if (!(info.IsClass && typeof(IAggregateRoot).IsAssignableFrom(type))) return;

            var genericType = typeof(AggregateRootRepository<>).MakeGenericType(type);
            var createFactory = ActivatorUtilities.CreateFactory(genericType, Type.EmptyTypes);
            _cacheAggregateRootRepo.TryAdd(type, (IAggregateRootRepository)createFactory(_serviceProvider, arguments: null));
        }

        private void RegisterExecutor(IEnumerable<Type> types)
        {
            foreach (var item in types)
            {
                this.RegisterExecutor(item);
            }
        }

        public IAggregateRootRepository GetAggregateRootRepository(Type aggregateType)
        {
            IAggregateRootRepository repo;
            if (_cacheAggregateRootRepo.TryGetValue(aggregateType, out repo)) return repo;

            // var genericType = typeof(AggregateRootRepository<>).MakeGenericType(aggregateType);
            // repo = this._serviceProvider.GetService(genericType) as IAggregateRootRepository;

            throw new NotImplementedException($"没有找到类型：{aggregateType.Name}");
        }
    }
}