using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Rainbow.DomainDriven.Event;
using System.Linq;
using MongoDB.Bson.Serialization.Conventions;

namespace Rainbow.DomainDriven.Mongo.Framework
{
    public class MongoDatabaseInitializer : IDatabaseInitializer
    {
        private List<Type> events = new List<Type>();
        private List<Type> roots = new List<Type>();

        private AssemblyProvider _assemblyProvider;


        public MongoDatabaseInitializer()
        {
            _assemblyProvider = new AssemblyProvider();

        }

        public bool IsRun { get; set; }

        public void Initialize()
        {
            var serializer = new DateTimeSerializer(DateTimeKind.Local);

            BsonSerializer.RegisterSerializer(typeof(DateTime), serializer);

            this.Register(_assemblyProvider.Assemblys.SelectMany(p => p.GetTypes()));

            Type[] types = events.Union(roots).ToArray();

            foreach (var item in types)
            {
                if (!BsonClassMap.IsClassMapRegistered(item))
                {
                    var classmap = new BsonClassMap(item);
                    classmap.AutoMap();
                    BsonClassMap.RegisterClassMap(classmap);
                }
            }
            IsRun = true;
        }




        protected virtual bool IsAggregateRoot(Type type)
        {
            if (!typeof(IAggregateRoot).IsAssignableFrom(type) || !type.IsClass) return false;
            var typeinfo = type.GetTypeInfo();
            return typeinfo.IsClass && !typeinfo.IsAbstract;

        }



        protected virtual bool IsEvent(Type type)
        {
            if (!typeof(IEvent).IsAssignableFrom(type) || !type.IsClass) return false;
            var typeinfo = type.GetTypeInfo();
            return typeinfo.IsClass && !typeinfo.IsAbstract;
        }

        private void Register(Type type)
        {
            if (IsEvent(type))
            {
                events.Add(type);
            }
            if (IsAggregateRoot(type))
            {
                roots.Add(type);
            }
        }



        private void Register(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                this.Register(type);
            }
        }
    }
}
