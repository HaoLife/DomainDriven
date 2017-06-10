using System;

namespace Rainbow.DomainDriven.RingQueue.Message
{
    public interface IReplyMessageStore
    {
        bool Contains(string key);

        bool TryGet(string key, out ReplyMessage replyMessage);

        void Add(ReplyMessage replyMessage);

        void RegisterChangeCallback(string key, Action<ReplyMessage> callback);
    }
}