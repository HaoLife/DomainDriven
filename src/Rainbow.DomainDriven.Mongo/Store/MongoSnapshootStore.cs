using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Store;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Rainbow.DomainDriven.Mongo.Store
{
    public class MongoSnapshootStore<TAggregateRoot> : ISnapshootStore<TAggregateRoot>, IConfigureChange, ISnapshootStore
        where TAggregateRoot : IAggregateRoot
    {
        private MongoSnapshootStoreFactory _factory;
        private IMongoCollection<TAggregateRoot> _collection;
        private BulkWriteOptions _options = new BulkWriteOptions() { IsOrdered = false };

        public MongoSnapshootStore(MongoSnapshootStoreFactory factory)
        {
            this._factory = factory;
            Reload();
        }

        public void Reload()
        {
            _collection = _factory.MongoDatabase.GetCollection<TAggregateRoot>(typeof(TAggregateRoot).Name);
        }

        public void Add(TAggregateRoot aggregate)
        {
            _collection.InsertOne(aggregate);
        }

        public void Add(TAggregateRoot[] aggregates)
        {
            _collection.InsertMany(aggregates);
        }


        public void Remove(TAggregateRoot aggregate)
        {
            var filter = Builders<TAggregateRoot>.Filter.Eq(a => a.Id, aggregate.Id);
            _collection.DeleteOne(filter);
        }

        public void Remove(TAggregateRoot[] aggregates)
        {
            List<WriteModel<TAggregateRoot>> writeModels = new List<WriteModel<TAggregateRoot>>();
            foreach (var aggregate in aggregates)
            {
                writeModels.Add(
                    new DeleteOneModel<TAggregateRoot>(Builders<TAggregateRoot>.Filter.Eq(a => a.Id, aggregate.Id))
                    );
            }

            _collection.BulkWrite(writeModels, _options);

        }

        public void Update(TAggregateRoot aggregate)
        {
            var filter = Builders<TAggregateRoot>.Filter.Eq(a => a.Id, aggregate.Id);
            _collection.ReplaceOne(filter, aggregate);
        }

        public void Update(TAggregateRoot[] aggregates)
        {
            List<WriteModel<TAggregateRoot>> writeModels = new List<WriteModel<TAggregateRoot>>();
            foreach (var aggregate in aggregates)
            {
                writeModels.Add(
                    new ReplaceOneModel<TAggregateRoot>(Builders<TAggregateRoot>.Filter.Eq(a => a.Id, aggregate.Id), aggregate)
                    );
            }

            _collection.BulkWrite(writeModels, _options);
        }

        public TAggregateRoot Get(Guid id)
        {
            return _collection.Find(a => a.Id == id).FirstOrDefault();
        }

        public List<TAggregateRoot> Get(Guid[] ids)
        {
            return _collection.Find(a => ids.Contains(a.Id)).ToList();
        }

        public void Add(IAggregateRoot aggregate)
        {
            if (aggregate == null || !aggregate.GetType().Equals(typeof(TAggregateRoot)))
                throw new Exception($"类型不是{typeof(TAggregateRoot).Name}");

            this.Add((TAggregateRoot)aggregate);
        }

        public void Add(IAggregateRoot[] aggregates)
        {
            if (!aggregates.All(a => a.GetType().Equals(typeof(TAggregateRoot))))
                throw new Exception($"类型不是{typeof(TAggregateRoot).Name}");
            var roots = aggregates.Select(a => (TAggregateRoot)a).ToArray();

            this.Add(roots);
        }

        public void Update(IAggregateRoot aggregate)
        {
            if (aggregate == null || !aggregate.GetType().Equals(typeof(TAggregateRoot)))
                throw new Exception($"类型不是{typeof(TAggregateRoot).Name}");

            this.Update((TAggregateRoot)aggregate);
        }

        public void Update(IAggregateRoot[] aggregates)
        {
            if (!aggregates.All(a => a.GetType().Equals(typeof(TAggregateRoot))))
                throw new Exception($"类型不是{typeof(TAggregateRoot).Name}");
            var roots = aggregates.Select(a => (TAggregateRoot)a).ToArray();

            this.Update(roots);
        }

        public void Remove(IAggregateRoot aggregate)
        {
            if (aggregate == null || !aggregate.GetType().Equals(typeof(TAggregateRoot)))
                throw new Exception($"类型不是{typeof(TAggregateRoot).Name}");

            this.Remove((TAggregateRoot)aggregate);
        }

        public void Remove(IAggregateRoot[] aggregates)
        {
            if (!aggregates.All(a => a.GetType().Equals(typeof(TAggregateRoot))))
                throw new Exception($"类型不是{typeof(TAggregateRoot).Name}");
            var roots = aggregates.Select(a => (TAggregateRoot)a).ToArray();

            this.Remove(roots);
        }

        IAggregateRoot ISnapshootStore.Get(Guid id)
        {
            return this.Get(id);
        }

        List<IAggregateRoot> ISnapshootStore.Get(Guid[] ids)
        {
            return this.Get(ids).Select(a => (IAggregateRoot)a).ToList();
        }
    }
}
