using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rainbow.DomainDriven.Mongo;
using Rainbow.DomainDriven.Mongo.Internal;
using Rainbow.DomainDriven.Mongo.Repository;
using Rainbow.DomainDriven.Repository;
using Rainbow.DomainDriven.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
        {

            services.TryAdd(new ServiceCollection()
                .AddSingleton<IMongoDatabaseProvider>(new MongoDatabaseProvider(configuration))
                .AddSingleton<IAggregateRootCommonQuery, MongoAggregateRootCommonQuery>()
                .AddSingleton(typeof(IAggregateRootQueryable<>), typeof(MongoAggregateRootQueryable<>))
                .AddSingleton<IEventSourceRepository, MongoEventSourceRepository>()
                .AddSingleton<IEventSourcingRepository, MongoEventSourcingRepository>()
                .AddSingleton<IDatabaseInitializer, MongoDatabaseInitializer>()
                .AddTransient<IAggregateRootOperation, AggregateRootOperation>()
                .AddTransient<IAggregateRootRepositoryContext, MongoAggregateRootRepositoryContext>()
                );

            return services;
        }
    }
}