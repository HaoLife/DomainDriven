using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Mongo
{
    public static class MongoServiceProviderExtensions
    {

        internal static IMongoDatabase CreateMongoDatabase(this IServiceProvider serviceProvider, IOptions<MongoOptions> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            var database = client.GetDatabase(options.Value.Database);
            return database;
        }
    }
}
