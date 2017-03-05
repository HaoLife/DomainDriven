using System;
using System.Collections.Concurrent;
using System.Threading;
using Rainbow.DomainDriven.RingQueue.Message;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public class MessageListening : IMessageListening
    {
        private readonly ConcurrentDictionary<string, ListeningMessage> _listenings;
        public MessageListening()
        {
            this._listenings = new ConcurrentDictionary<string, ListeningMessage>();
        }

        public void Notice(string key, NoticeMessage message)
        {
            var listening = this._listenings.GetOrAdd(key, p => new ListeningMessage());
            listening.Message = message;
            listening.Listening = 1;
        }

        public NoticeMessage WiatFor(string key)
        {
            var message = this._listenings.GetOrAdd(key, p => new ListeningMessage());
            var spinWait = default(SpinWait);

            while (message.Listening == 0)
            {
                spinWait.SpinOnce();
            }
            return message.Message;
        }
    }
}