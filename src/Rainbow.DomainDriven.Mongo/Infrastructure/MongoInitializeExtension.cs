using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.Collections.Generic;
using Rainbow.DomainDriven.Mongo.Repository;
using Rainbow.DomainDriven.Infrastructure;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;

namespace Rainbow.DomainDriven.Mongo.Infrastructure
{
    public class MongoInitializeExtension : IDomainInitializeExtension
    {
        public void ApplyServices(IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            Initialize(provider);
        }

        private void Initialize(IServiceProvider provider)
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