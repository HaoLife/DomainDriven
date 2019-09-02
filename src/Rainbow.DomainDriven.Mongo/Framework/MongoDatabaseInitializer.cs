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

            if (!BsonSerializer.IsTypeDiscriminated(typeof(DateTime)))
            {
                var serializer = new DateTimeSerializer(DateTimeKind.Local);
                BsonSerializer.RegisterSerializer(typeof(DateTime), serializer);
            }

            if (!BsonSerializer.IsTypeDiscriminated(typeof(decimal)))
            {
                var serializer = new DecimalSerializer(BsonType.Decimal128, new MongoDB.Bson.Serialization.Options.RepresentationConverter(true, true));
                BsonSerializer.RegisterSerializer(typeof(decimal), serializer);

            }

            ConventionRegistry.Register("IgnoreExtraElements", new ConventionPack { new IgnoreExtraElementsConvention(true) }, type => true);

            IsRun = true;
        }


    }
}
