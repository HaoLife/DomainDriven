namespace Rainbow.DomainDriven.Message
{
    public class MessageHead
    {
        public MessageHead()
        {

        }

        public MessageHead(string key, PriorityLevel priority, ConsistencyLevel consistency)
            : this(key, string.Empty, priority, consistency)
        {

        }

        public MessageHead(string key, string replyKey, PriorityLevel priority, ConsistencyLevel consistency)
        {
            this.Key = key;
            this.ReplyKey = replyKey;
            this.Priority = priority;
            this.Consistency = consistency;
        }
        public string Key { get; set; }
        public string ReplyKey { get; set; }
        public PriorityLevel Priority { get; set; }
        public ConsistencyLevel Consistency { get; set; }
    }
}