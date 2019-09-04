using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Rainbow.DomainDriven.Event;
using System.Linq;
using MongoDB.Bson.Serialization.Conventions;
using Rainbow.DomainDriven.Framework;
using MongoDB.Bson;
using System.Collections.Concurrent;
using MongoDB.Bson.Serialization.Options;

namespace Rainbow.DomainDriven.Mongo.Framework
{
    public class MongoDatabaseInitializer : IDatabaseInitializer
    {
        private List<Type> events = new List<Type>();
        private List<Type> roots = new List<Type>();

        private IAssemblyProvider _assemblyProvider;


        public MongoDatabaseInitializer(IAssemblyProvider assemblyProvider)
        {
            _assemblyProvider = assemblyProvider;

        }

        public bool IsRun { get; set; }

        public void Initialize()
        {

            this.AddOrUpdate(typeof(decimal), new DecimalSerializer(BsonType.Decimal128, new RepresentationConverter(true, true)));
            this.AddOrUpdate(typeof(DateTime), new DateTimeSerializer(DateTimeKind.Local));

            ConventionRegistry.Register("IgnoreExtraElements", new ConventionPack { new IgnoreExtraElementsConvention(true) }, type => true);

            IsRun = true;
        }

        private void AddOrUpdate(Type type, IBsonSerializer serializer)
        {
            var currentSerializer = BsonSerializer.LookupSerializer(type);
            if (currentSerializer != null)
            {
                // remove exist
                var cacheFieldInfo = typeof(BsonSerializerRegistry).
                    GetField("_cache", BindingFlags.NonPublic |BindingFlags.Instance);

                var _cacheTemp = cacheFieldInfo.GetValue(BsonSerializer.SerializerRegistry);
                var _cache = _cacheTemp as ConcurrentDictionary<Type, IBsonSerializer>;
                IBsonSerializer removeed;
                _cache.TryRemove(type, out removeed);

                // add my owner
            }
            BsonSerializer.RegisterSerializer(type, serializer);

        }
    }
}
