namespace Rainbow.DomainDriven.Message
{
    public class DomainMessage
    {
        public DomainMessage()
        {

        }
        public DomainMessage(MessageHead head, object content)
        {
            this.Head = head;
            this.Content = content;
        }

        public MessageHead Head { get; set; }
        public object Content { get; set; }
    }
}