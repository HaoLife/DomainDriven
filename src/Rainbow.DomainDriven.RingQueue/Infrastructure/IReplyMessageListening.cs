using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public interface IReplyMessageListening
    {
        bool Contains(string key);

        bool TryGet(string key, out ReplyMessage replyMessage);

        void Add(ReplyMessage replyMessage);

        ReplyMessage ForWait(string key, int millisecondsTimeout = 10000);
    }
}