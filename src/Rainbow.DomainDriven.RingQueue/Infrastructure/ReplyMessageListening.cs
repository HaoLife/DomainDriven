using System;
using System.Collections.Concurrent;
using System.Threading;
using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public class ReplyMessageListening : IReplyMessageListening
    {
        private readonly ConcurrentDictionary<string, ReplyMessage> _replyMessages;
        public ReplyMessageListening()
        {
            this._replyMessages = new ConcurrentDictionary<string, ReplyMessage>();
        }

        public void Add(ReplyMessage replyMessage)
        {
            this._replyMessages.TryAdd(replyMessage.ReplyKey, replyMessage);
        }

        public bool Contains(string key)
        {
            return this._replyMessages.ContainsKey(key);
        }

        public ReplyMessage ForWait(string key, int millisecondsTimeout = 10000)
        {
            ReplyMessage replyMessage = null;
            if (!SpinWait.SpinUntil(() => this.TryGet(key, out replyMessage), millisecondsTimeout))
            {
                throw new TimeoutException("等待超时");
            }
            return replyMessage;
        }
        
        public bool TryGet(string key, out ReplyMessage replyMessage)
        {
            return this.TryGet(key, out replyMessage);
        }
    }
}