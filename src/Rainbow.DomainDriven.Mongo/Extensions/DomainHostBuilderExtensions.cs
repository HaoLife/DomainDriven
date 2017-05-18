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
using Rainbow.DomainDriven.Host;

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
                .AddSingleton<IAggregateRootQuery, MongoCommonQueryRepository>()
                .AddSingleton<IAggregateRootLockRetoryProvider, AggregateRootLockRepositoryProvider>()
                .AddSingleton(typeof(AggregateRootLockRepository<>))
                .AddSingleton<IAggregateRootBatchRepositoryProvider, AggregateRootBatchRepositoryProvider>()
                .AddSingleton(typeof(AggregateRootBatchRepository<>))
                .AddSingleton(typeof(IAggregateRootQueryable<>), typeof(AggregateRootQueryRepository<>))
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
