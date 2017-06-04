using System;
using Rainbow.DomainDriven.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;

namespace Rainbow.DomainDriven.Mongo
{
    public class MongoDatabaseInitializer : IDatabaseInitializer
    {
        public void Initialize(IServiceProvider provider)
        {
            var domainTypeProvider = provider.GetRequiredService<IDomainTypeProvider>();
            
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