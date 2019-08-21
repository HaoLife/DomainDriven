using Microsoft.Extensions.Options;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.RingQueue.Framework;
using Rainbow.MessageQueue.Ring;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Rainbow.DomainDriven.Store;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Domain;
using System.Linq;
using System.Threading;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class RingEventBus : IEventBus
    {
        private ILogger _logger;
        private IRingBufferProcess _ringBufferProcess;


        public RingEventBus(
            IRingBufferProcess ringBufferProcess
            , ILoggerFactory loggerFactory)
        {
            _ringBufferProcess = ringBufferProcess;
            _logger = loggerFactory.CreateLogger<RingEventBus>();
        }

        public void Publish(IEvent[] events)
        {
            if (!_ringBufferProcess.IsStart)
            {
                throw new Exception("队列执行程序未启动，无法发送事件到事件执行队列中");
            }
            var queue = _ringBufferProcess.GetEvent();

            _logger.LogDebug($"发送事件:{events.Length}");
            if (events.Length > queue.Size)
            {
                SplitPublish(queue, events);
                return;
            }
            AllPublish(queue, events);

        }

        private void AllPublish(RingBuffer<IEvent> queue, IEvent[] events)
        {
            var seq = queue.Next(events.Length);
            var index = seq - events.Length + 1;
            var start = index;
            while (index <= seq)
            {
                queue[index].Value = events[index - start];
                queue.Publish(index);
                index++;
            }
        }

        private void SplitPublish(RingBuffer<IEvent> queue, IEvent[] events)
        {
            int skip = 0;
            int take = queue.Size / 2;
            IEvent[] evs = events.Skip(skip).Take(take).ToArray();
            do
            {
                AllPublish(queue, evs);
                skip += evs.Length;
                evs = events.Skip(skip).Take(take).ToArray();

            } while (evs.Length > 0);
        }
    }


}
