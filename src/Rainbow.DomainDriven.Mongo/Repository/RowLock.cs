using System;

namespace Rainbow.DomainDriven.Mongo.Repository
{
    public class RowLock
    {
        public RowLock(long expireTicks)
        {
            this.Value = expireTicks.ToString();
            this.Expires = expireTicks;
        }
        public RowLock(string value, long expireTicks)
        {
            this.Value = value;
            this.Expires = expireTicks;
        }
        public string Value { get; set; }
        public long Expires { get; set; }
    }
}