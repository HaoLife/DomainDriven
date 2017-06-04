namespace Rainbow.DomainDriven.Message
{
    public class MessageHead
    {
        public MessageHead() { }
        public MessageHead(Priority priority, Consistency consistency) : this(string.Empty, priority, consistency) { }
        public MessageHead(string replyTo, Priority priority, Consistency consistency)
        {
            this.ReplyTo = replyTo;
            this.Priority = priority;
            this.Consistency = consistency;
        }
        public string ReplyTo { get; set; }
        public Priority Priority { get; set; }
        public Consistency Consistency { get; set; }
    }
}