using System;
using System.Collections.Concurrent;

namespace Rainbow.DomainDriven.RingQueue.Message
{
    public class ReplyMessageStore : IReplyMessageStore
    {
        public class ReplyStoreContent
        {
            public ReplyMessage Message { get; set; }
            public Action<ReplyMessage> Callback { get; set; }
        }
        private readonly ConcurrentDictionary<string, ReplyStoreContent> _replyMessages;
        public ReplyMessageStore()
        {
            this._replyMessages = new ConcurrentDictionary<string, ReplyStoreContent>();
        }

        public void Add(ReplyMessage replyMessage)
        {
            var content = this._replyMessages.AddOrUpdate(replyMessage.ReplyKey,
                a => new ReplyStoreContent() { Message = replyMessage },
                (a, b) => { b.Message = replyMessage; return b; });

            if (content.Callback != null)
            {
                content.Callback(content.Message);
            }
        }

        public bool Contains(string key)
        {
            return this._replyMessages.ContainsKey(key);
        }

        public bool TryGet(string key, out ReplyMessage replyMessage)
        {
            ReplyStoreContent content;
            replyMessage = null;
            var result = this._replyMessages.TryGetValue(key, out content);
            if (!result) return false;
            replyMessage = content.Message;
            return true;
        }


        public void RegisterChangeCallback(string key, Action<ReplyMessage> callback)
        {
            var content = this._replyMessages.AddOrUpdate(key,
                a => new ReplyStoreContent() { Callback = callback },
                (a, b) => { b.Callback = callback; return b; });

            if (content.Message != null)
            {
                callback(content.Message);
            }
        }
    }
}