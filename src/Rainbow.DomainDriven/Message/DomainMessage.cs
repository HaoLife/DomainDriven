namespace Rainbow.DomainDriven.Message
{
    public class DomainMessage<TMessage>
    {
        public DomainMessage() { }
        public DomainMessage(MessageHead head, TMessage content)
        {
            this.Head = head;
            this.Content = content;
        }
        public MessageHead Head { get; set; }
        public TMessage Content { get; set; }
    }
}