using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rainbow.DomainDriven.Mongo
{
    public class MongoOptions
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }

        public string EventSourceConnectionString { get; set; }
        public string EventSourceDatabase { get; set; }
    }
}
