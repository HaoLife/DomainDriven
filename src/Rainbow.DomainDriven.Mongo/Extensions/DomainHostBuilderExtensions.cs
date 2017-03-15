using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Rainbow.DomainDriven.Mongo;
using Rainbow.DomainDriven;
using Rainbow.DomainDriven.Repository;
using Rainbow.DomainDriven.Mongo.Repository;
using Rainbow.DomainDriven.Mongo.Internal;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.Mongo.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DomainHostBuilderExtensions
    {
        public static IDomainHostBuilder UseTranMongoAggregateRootRepository(this IDomainHostBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddOptions()
                .AddMemoryCache();

            builder.Services
                .Configure<MongoOptions>(configuration);

            builder.Services.TryAdd(new ServiceCollection()
                .AddTransient(p => p.CreateMongoDatabase(p.GetService<IOptions<MongoOptions>>()))
                .AddSingleton<IAggregateRootRepositoryContext, MongoLockAggregateRootRepositoryContext>()
                .AddSingleton<IAggregateRootCommonQueryRepository, MongoCommonQueryRepository>()
                .AddSingleton<IAggregateRootLockRepositoryProvider, AggregateRootLockRepositoryProvider>()
                .AddSingleton(typeof(AggregateRootLockRepository<>))
                .AddSingleton<IAggregateRootBatchRepositoryProvider, AggregateRootBatchRepositoryProvider>()
                .AddSingleton(typeof(AggregateRootBatchRepository<>))
                .AddSingleton(typeof(IAggregateRootQueryRepository<>), typeof(AggregateRootQueryRepository<>))
                .AddSingleton<IAggregateRootRepositoryProvider, AggregateRootRepositoryProvider>()
                .AddSingleton<IEventSourceRepository, MongoEventSourceRepository>()
                .AddSingleton<IEventSourcingRepository, MongoEventSourcingRepository>()
                .AddTransient<IAggregateRootOperation, AggregateRootOperation>()
                );

            builder.ApplyServices(new MongoInitializeExtension());
            return builder;

        }
    }
}
