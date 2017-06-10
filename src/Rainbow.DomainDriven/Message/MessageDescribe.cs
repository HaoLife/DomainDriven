namespace Rainbow.DomainDriven.Message
{
    public class MessageDescribe
    {
        public MessageDescribe(Priority priority, Consistency consistency)
        {
            this.Priority = priority;
            this.Consistency = consistency;
        }
        public Priority Priority { get; set; }
        public Consistency Consistency { get; set; }

        public override string ToString()
        {
            return $"{this.Consistency}_{this.Priority}";
        }

        public override int GetHashCode()
        {
            return this.Priority.GetHashCode() + (this.Consistency.GetHashCode() << 4);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            return obj.GetHashCode() == this.GetHashCode();
        }

    }
}