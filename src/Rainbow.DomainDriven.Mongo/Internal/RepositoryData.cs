using Rainbow.DomainDriven.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Mongo.Internal
{
    internal class RepositoryData<TAggregateRoot> where TAggregateRoot : class, IAggregateRoot
    {
        public StoreType State { get; set; }
        public TAggregateRoot Entity { get; set; }
    }
}
