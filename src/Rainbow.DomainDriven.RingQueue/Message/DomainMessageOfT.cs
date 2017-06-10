using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.RingQueue.Message
{
    public class DomainMessage<TMessage>
    {
        public MessageDescribe MessageDescribe { get; set; }
        public TMessage Content { get; set; }
        public string ReplyKey { get; set; }
    }
}