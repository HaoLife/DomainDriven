using MongoDB.Driver.Linq;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Store;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MongoDB.Driver;

namespace Rainbow.DomainDriven.Mongo.Store
{
    public class MongoSnapshootQuery<TAggregateRoot> : ISnapshootQuery<TAggregateRoot>, IConfigureChange
        where TAggregateRoot : IAggregateRoot
    {
        private MongoSnapshootQueryFactory _factory;
        private IMongoQueryable<TAggregateRoot> _collection;

        public MongoSnapshootQuery(MongoSnapshootQueryFactory factory)
        {
            this._factory = factory;
            Reload();
        }

        public Type ElementType => _collection.ElementType;

        public Expression Expression => _collection.Expression;

        public IQueryProvider Provider => _collection.Provider;

        public IEnumerator<TAggregateRoot> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public void Reload()
        {
            _collection = _factory.MongoDatabase.GetCollection<TAggregateRoot>(typeof(TAggregateRoot).Name).AsQueryable();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
    }
}
