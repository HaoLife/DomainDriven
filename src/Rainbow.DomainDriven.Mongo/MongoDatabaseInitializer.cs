using System;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Rainbow.DomainDriven.Infrastructure;

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

            foreach (var item in domainTypeProvider.Events)
            {
                if (!BsonClassMap.IsClassMapRegistered(item))
                {
                    var map = new BsonClassMap(item);
                    map.AutoMap();
                    BsonClassMap.RegisterClassMap(map);
                }
            }
        }
    }
}