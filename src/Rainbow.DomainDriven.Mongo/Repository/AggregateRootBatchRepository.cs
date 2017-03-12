using System;
using System.Collections.Generic;
using MongoDB.Driver;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Repository;
using System.Linq;
using Rainbow.DomainDriven.Mongo.Internal;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class AggregateRootBatchRepository<TAggregateRoot>
        : IAggregateRootBatchRepository
        where TAggregateRoot : class, IAggregateRoot
    {

        private readonly IMongoCollection<TAggregateRoot> _mongoCollection;
        private readonly IAggregateRootOperation _aggregateRootOperation;
        private readonly List<WriteModel<TAggregateRoot>> _unCommit;
        public AggregateRootBatchRepository(
            IMongoDatabase mongoDatabase,
            IAggregateRootOperation aggregateRootOperation)
        {
            this._mongoCollection = mongoDatabase.GetCollection<TAggregateRoot>(typeof(TAggregateRoot).Name);
            this._aggregateRootOperation = aggregateRootOperation;
            this._unCommit = new List<WriteModel<TAggregateRoot>>();
        }
        public void Add(IEnumerable<IAggregateRoot> roots)
        {
            if (!roots.Any()) return;
            foreach (var item in roots)
            {
                this.Add(item);
            }
        }

        public void Add(IAggregateRoot root)
        {
            this._aggregateRootOperation.Add(root as TAggregateRoot);
        }

        public void Commit()
        {
            var list = new List<WriteModel<TAggregateRoot>>();
            this.BuildAdded(list, this._aggregateRootOperation.GetAdded(typeof(TAggregateRoot)));
            this.BuildUpdated(list, this._aggregateRootOperation.GetUpdated(typeof(TAggregateRoot)));
            this.BuildRemoved(list, this._aggregateRootOperation.GetRemoved(typeof(TAggregateRoot)));
            this._mongoCollection.BulkWrite(list);
        }

        public void BuildAdded(List<WriteModel<TAggregateRoot>> list, IEnumerable<IAggregateRoot> roots)
        {
            foreach (var item in roots)
            {
                list.Add(new InsertOneModel<TAggregateRoot>(item as TAggregateRoot));
            }
        }

        public void BuildUpdated(List<WriteModel<TAggregateRoot>> list, IEnumerable<IAggregateRoot> roots)
        {
            var filter = Builders<TAggregateRoot>.Filter;
            foreach (var item in roots)
            {
                var query = filter.And(
                        filter.Eq(p => p.Id, item.Id),
                        filter.Eq(p => p.Version, item.Version - 1)
                    );
                list.Add(new ReplaceOneModel<TAggregateRoot>(query, item as TAggregateRoot));
            }
        }

        public void BuildRemoved(List<WriteModel<TAggregateRoot>> list, IEnumerable<IAggregateRoot> roots)
        {
            var filter = Builders<TAggregateRoot>.Filter;
            foreach (var item in roots)
            {
                var query = filter.And(
                        filter.Eq(p => p.Id, item.Id),
                        filter.Eq(p => p.Version, item.Version - 1)
                    );
                list.Add(new DeleteOneModel<TAggregateRoot>(query));
            }
        }

        public IEnumerable<IAggregateRoot> Get(params Guid[] keys)
        {
            List<IAggregateRoot> ret = new List<IAggregateRoot>();
            foreach (var key in keys)
            {
                var root = this._aggregateRootOperation.Get(typeof(TAggregateRoot), key);
                if (root == null) continue;
                ret.Add(root);
            }
            var ids = keys.Intersect(ret.Select(a => a.Id));
            if (!ids.Any()) return ret;
            var data = this._mongoCollection.Find(a => ids.Contains(a.Id));
            ret.AddRange(data.ToList());
            return ret;
        }

        public IAggregateRoot Get(Guid id)
        {
            var root = this._aggregateRootOperation.Get(typeof(TAggregateRoot), id);
            if (root != null) return root;

            return this._mongoCollection.Find(a => a.Id == id).FirstOrDefault();
        }

        public void Remove(IEnumerable<IAggregateRoot> roots)
        {
            if (!roots.Any()) return;
            foreach (var item in roots)
            {
                this.Remove(item);
            }
        }

        public void Remove(IAggregateRoot root)
        {
            this._aggregateRootOperation.Remove(root as TAggregateRoot);
        }

        public void Update(IEnumerable<IAggregateRoot> roots)
        {
            if (!roots.Any()) return;
            foreach (var item in roots)
            {
                this.Update(item);
            }
        }

        public void Update(IAggregateRoot root)
        {
            this._aggregateRootOperation.Update(root as TAggregateRoot);
        }

    }
}