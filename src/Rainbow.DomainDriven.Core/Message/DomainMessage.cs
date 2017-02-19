namespace Rainbow.DomainDriven.Message
{
    public class DomainMessage<TContent>
    {
        public DomainMessage()
        {

        }
        public DomainMessage(MessageHead head, TContent content)
        {
            this.Head = head;
            this.Content = content;
        }

        public MessageHead Head { get; set; }
        public TContent Content { get; set; }
    }
}