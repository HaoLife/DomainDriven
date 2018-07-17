using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Rainbow.DomainDriven.Framework;
using Rainbow.DomainDriven.Mongo.Framework;
using Rainbow.DomainDriven.Mongo.Store;
using Rainbow.DomainDriven.Store;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MongoDomainBuilderExtensions
    {
        public static IDomainBuilder AddMongo(this IDomainBuilder builder, IConfiguration configuration)
        {

            builder.AddMongoOptions(configuration);

            builder.Services.TryAdd(new ServiceCollection()
                .AddSingleton<IEventStore, MongoEventStore>()
                .AddSingleton<ISubscribeEventStore, MongoSubscribeEventStore>()
                .AddSingleton<ISnapshootQueryFactory, MongoSnapshootQueryFactory>()
                .AddSingleton<ISnapshootStoreFactory, MongoSnapshootStoreFactory>()
                );


            return builder;
        }

        internal static IDomainBuilder AddMongoOptions(this IDomainBuilder builder, IConfiguration configuration)
        {
            builder.Services.TryAdd(new ServiceCollection()
                .AddSingleton<IConfigureOptions<MongoOptions>>(new MongoConfigureOptions(configuration))
                .AddSingleton<IOptionsChangeTokenSource<MongoOptions>>(new ConfigurationChangeTokenSource<MongoOptions>(configuration))

                );

            return builder;
        }
    }
}
