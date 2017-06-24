using System;
using System.Collections.Generic;
using System.Threading;

namespace Rainbow.DomainDriven.RingQueue.Message
{
    public class MessageMonitor : IMessageMonitor
    {
        public class MonitorMessage
        {
            public int Status { get; set; }
            public ReplyMessage Message { get; set; }
        }
        private readonly IReplyMessageStore _replyMessageStore;
        private Dictionary<string, MonitorMessage> _monitorCache;
        public MessageMonitor(IReplyMessageStore replyMessageStore)
        {
            this._replyMessageStore = replyMessageStore;
            this._monitorCache = new Dictionary<string, MonitorMessage>();

        }
        public void Wait(string key)
        {
            var message = new MonitorMessage();
            this._monitorCache.Add(key, message);
            this._replyMessageStore.RegisterChangeCallback(key, Callback);
            SpinWait.SpinUntil(() => message.Status == 1);
            if (!message.Message.IsSuccess) throw message.Message.Exception;
        }

        public void Callback(ReplyMessage message)
        {
            if (this._monitorCache.TryGetValue(message.ReplyKey, out MonitorMessage value))
            {
                value.Status = 1;
                value.Message = message;
            }
        }
    }
}