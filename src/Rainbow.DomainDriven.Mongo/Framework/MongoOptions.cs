using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.DomainDriven.Mongo.Framework
{
    public class MongoOptions
    {
        public string SnapshootConnection { get; set; }
        public string SnapshootDbName { get; set; }

        public string EventConnection { get; set; }
        public string EventDbName { get; set; }


        public string EventName { get; set; } = "events";
        public string SubscribeEventName { get; set; } = "subscribes";

    }
}
