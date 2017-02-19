using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Rainbow.DomainDriven.Mongo;
using Rainbow.DomainDriven;
using MongoDB.Driver;
using Rainbow.DomainDriven.Repository;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Repository;
using Rainbow.DomainDriven.Mongo.Repository;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DomainHostBuilderExtensions
    {
        public static IDomainHostBuilder UseTranMongoAggregateRootRepository(this IDomainHostBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddOptions()
                .AddMemoryCache();

            builder.Services
                .Configure<DomainOptions>(a => a.Add(new MongoInitializeExtension()))
                .Configure<MongoOptions>(configuration);

            builder.Services.TryAdd(new ServiceCollection()
                .AddTransient(p => p.CreateMongoDatabase(p.GetService<IOptions<MongoOptions>>()))
                .AddSingleton<IAggregateRootRepositoryContext, MongoLockAggregateRootRepositoryContext>()
                .AddSingleton<IAggregateRootCommonQueryRepository, MongoCommonQueryRepository>()
                .AddSingleton(typeof(IAggregateRootQueryRepository<>), typeof(AggregateRootRepository<>))
                );

            return builder;

        }
    }
}
