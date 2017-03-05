using Rainbow.DomainDriven.RingQueue.Message;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public interface IMessageListening
    {
        NoticeMessage WiatFor(string key);

        void Notice(string key, NoticeMessage message);
    }
}