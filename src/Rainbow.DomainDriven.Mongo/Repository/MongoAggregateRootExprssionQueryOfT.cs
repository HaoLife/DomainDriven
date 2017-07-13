using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rainbow.DomainDriven.Repository;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class MongoAggregateRootExprssionQueryOfT<TAggregateRoot>
        : IAggregateRootExprssionQueryOfT<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot
    {
        private readonly IMongoQueryable<TAggregateRoot> _collection;
        public MongoAggregateRootExprssionQueryOfT(IMongoDatabaseProvider mongoDatabaseProvider)
        {
            this._collection = mongoDatabaseProvider.GetCollection<TAggregateRoot>(typeof(TAggregateRoot).Name).AsQueryable();
        }
        public Type ElementType => _collection.ElementType;

        public Expression Expression => _collection.Expression;

        public IQueryProvider Provider => _collection.Provider;

        public IEnumerator<TAggregateRoot> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
    }
}