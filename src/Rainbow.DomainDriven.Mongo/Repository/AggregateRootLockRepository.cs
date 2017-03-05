using MongoDB.Driver;
using Rainbow.DomainDriven.Domain;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Net;
using System.Net.Sockets;

namespace Rainbow.DomainDriven.Mongo.Repository
{

    public class AggregateRootLockRepository<TAggregateRoot>
        : IAggregateRootLockRepository
        where TAggregateRoot : class, IAggregateRoot
    {

        private readonly IMongoCollection<TAggregateRoot> _mongoCollection;
        private string _ipAddress;
        public AggregateRootLockRepository(IMongoDatabase mongoDatabase)
        {
            this._mongoCollection = mongoDatabase.GetCollection<TAggregateRoot>(typeof(TAggregateRoot).Name);
            Init();
        }

        private void Init()
        {
            var task = Dns.GetHostAddressesAsync(Environment.MachineName);
            task.Wait();
            var ip = task.Result.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            this._ipAddress = Guid.NewGuid().ToShort();
            if (ip != null)
            {
                this._ipAddress = ip.ToString();
            }
        }

        public void UnLock(IEnumerable<IAggregateRoot> roots)
        {
            if (!roots.Any()) return;
            var filter = Builders<TAggregateRoot>.Filter;
            var updater = Builders<TAggregateRoot>.Update;
            var rowField = new StringFieldDefinition<TAggregateRoot, string>("Row");
            var expiresField = new StringFieldDefinition<TAggregateRoot, long>("Expires");
            var list = new List<WriteModel<TAggregateRoot>>();
            foreach (var item in roots)
            {
                var query = filter.And(
                        filter.Eq(p => p.Id, item.Id)
                    );

                var update = updater.Unset(rowField).Unset(expiresField);
                list.Add(new UpdateOneModel<TAggregateRoot>(query, update));
            }

            var model = this._mongoCollection.BulkWrite(list);
        }


        public void Lock(IEnumerable<IAggregateRoot> roots, long expires)
        {
            if (!roots.Any()) return;
            var filter = Builders<TAggregateRoot>.Filter;
            var updater = Builders<TAggregateRoot>.Update;
            var rowField = new StringFieldDefinition<TAggregateRoot, string>("Row");
            var expiresField = new StringFieldDefinition<TAggregateRoot, long>("Expires");
            var list = new List<WriteModel<TAggregateRoot>>();
            foreach (var item in roots)
            {
                var query = filter.Or(
                    filter.And(
                        filter.Eq(p => p.Id, item.Id),
                        filter.Eq(rowField, string.Empty)
                    ),
                    filter.And(
                        filter.Eq(p => p.Id, item.Id),
                        filter.Gte(expiresField, expires)
                    )
                );
                var update = updater.Set(rowField, _ipAddress).Set(expiresField, expires);
                list.Add(new UpdateOneModel<TAggregateRoot>(query, update));
            }

            var model = this._mongoCollection.BulkWrite(list);
            if (model.Upserts.Count != list.Count)
                throw new LockException("无法锁定所有对象");
        }

        public void Add(IAggregateRoot root)
        {
            this._mongoCollection.InsertOne(root as TAggregateRoot);
        }

        public void Update(IAggregateRoot root)
        {
            var filters = Builders<TAggregateRoot>.Filter;
            var query = filters.Eq(p => p.Id, root.Id);
            this._mongoCollection.ReplaceOne(query, root as TAggregateRoot);
        }

        public void Remove(IAggregateRoot root)
        {
            var filters = Builders<TAggregateRoot>.Filter;
            var query = filters.Eq(p => p.Id, root.Id);
            this._mongoCollection.DeleteOne(query);
        }

        public IAggregateRoot Get(Guid id)
        {
            return this._mongoCollection.Find(a => a.Id == id).FirstOrDefault();

        }

        public IEnumerable<IAggregateRoot> Get(params Guid[] keys)
        {
            return this._mongoCollection.Find(a => keys.Contains(a.Id)).ToList();
        }

        public void Add(IEnumerable<IAggregateRoot> roots)
        {
            this._mongoCollection.InsertMany(roots.Select(p => p as TAggregateRoot));
        }

        public void Update(IEnumerable<IAggregateRoot> roots)
        {
            List<ReplaceOneModel<TAggregateRoot>> list = new List<ReplaceOneModel<TAggregateRoot>>();
            var filter = Builders<TAggregateRoot>.Filter;
            foreach (var item in roots)
            {
                var query = filter.And(
                        filter.Eq(p => p.Id, item.Id),
                        filter.Eq(p => p.Version, item.Version)
                    );
                list.Add(new ReplaceOneModel<TAggregateRoot>(query, item as TAggregateRoot));
            }
            this._mongoCollection.BulkWrite(list);
        }

        public void Remove(IEnumerable<IAggregateRoot> roots)
        {
            List<DeleteOneModel<TAggregateRoot>> list = new List<DeleteOneModel<TAggregateRoot>>();
            var filter = Builders<TAggregateRoot>.Filter;
            foreach (var item in roots)
            {
                var query = filter.And(
                        filter.Eq(p => p.Id, item.Id),
                        filter.Eq(p => p.Version, item.Version)
                    );
                list.Add(new DeleteOneModel<TAggregateRoot>(query));
            }
        }
    }
}