using System;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Rainbow.DomainDriven.Infrastructure;
using System.Linq;

namespace Rainbow.DomainDriven.Mongo
{
    public class MongoDatabaseInitializer : IDatabaseInitializer
    {
        public void Initialize(IServiceProvider provider)
        {
            var assemblyProvider = new AssemblyProvider();
            var domainTypeProvider = new DomainTypeProvider(assemblyProvider);

            var serializer = new DateTimeSerializer(DateTimeKind.Local);
            BsonSerializer.RegisterSerializer(typeof(DateTime), serializer);
            Type[] types = domainTypeProvider.Events.Union(domainTypeProvider.AggregateRoots).ToArray();
            foreach (var item in types)
            {
                if (!BsonClassMap.IsClassMapRegistered(item))
                {
                    var classmap = new BsonClassMap(item);
                    classmap.AutoMap();
                    BsonClassMap.RegisterClassMap(classmap);
                }
            }
            

        }
    }
}