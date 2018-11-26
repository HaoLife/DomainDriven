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
using Rainbow.DomainDriven.Infrastructure;
using System.Threading.Tasks;
using Rainbow.DomainDriven.Domain;
using System.Linq;
using System.Threading;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class RingEventBus : IEventBus
    {
        private IServiceProvider _provider;
        private RingOptions _options;
        private IDisposable _optionsReloadToken;
        private List<IRingBufferConsumer> consumers = new List<IRingBufferConsumer>();
        private RingBuffer<IEvent> _handleQueue;
        private ILogger _logger;

        public RingEventBus(IOptionsMonitor<RingOptions> options, IServiceProvider provider)
        {
            _provider = provider;
            _optionsReloadToken = options.OnChange(ReloadOptions);
            ReloadOptions(options.CurrentValue);
            _logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger<RingEventBus>();
        }

        private void ReloadOptions(RingOptions options)
        {
            _options = options;
            Initialize();
        }


        private void Initialize()
        {
            consumers.ForEach(a => a.Halt());
            consumers.Clear();

            var snapshootStoreFactory = _provider.GetRequiredService<ISnapshootStoreFactory>();
            var loggerFactory = _provider.GetRequiredService<ILoggerFactory>();
            var eventHandlerFactory = _provider.GetRequiredService<IEventHandlerFactory>();
            //var memoryCache = _provider.GetRequiredService<IMemoryCache>();
            var snapshootCache = _provider.GetRequiredService<ISnapshootCache>();
            var eventRegister = _provider.GetRequiredService<IEventRegister>();
            var assemblyProvider = _provider.GetRequiredService<IAssemblyProvider>();
            var eventRebuildHandler = _provider.GetRequiredService<IEventRebuildHandler>();
            var subscribeEventStore = _provider.GetRequiredService<ISubscribeEventStore>();
            var eventHandleSubject = _provider.GetRequiredService<IEventHandleSubject>();

            var size = _options.EventQueueSize;



            IWaitStrategy wait = new SpinWaitStrategy();
            MultiSequencer sequencer = new MultiSequencer(size, wait);
            RingBuffer<IEvent> queue = new RingBuffer<IEvent>(sequencer);
            var barrier = queue.NewBarrier();

            var snapshootHandler = new RingEventSnapshootHandler(
                assemblyProvider
                , snapshootStoreFactory
                , eventRebuildHandler
                , subscribeEventStore
                , snapshootCache
                , loggerFactory
                , eventHandleSubject);
            IRingBufferConsumer snapshootConsumer = new RingBufferConsumer<IEvent>(
                queue,
                barrier,
                snapshootHandler);

            barrier = queue.NewBarrier(snapshootConsumer.Sequence);
            consumers.Add(snapshootConsumer);


            var executorHandler = new RingEventBusinessHandler(
                eventRegister
                , eventHandlerFactory
                , subscribeEventStore
                , loggerFactory
                , eventHandleSubject);
            IRingBufferConsumer executorConsumer = new RingBufferConsumer<IEvent>(
                queue,
                barrier,
                executorHandler);

            consumers.Add(executorConsumer);

            queue.AddGatingSequences(executorConsumer.Sequence);

            consumers.ForEach(a => Task.Factory.StartNew(a.Run, TaskCreationOptions.LongRunning));

            _handleQueue = queue;
        }

        public void Publish(IEvent[] events)
        {
            _logger.LogDebug($"发送事件:{events.Length}");
            if (events.Length > _handleQueue.Size)
            {
                SplitPublish(events);
                return;
            }
            AllPublish(events);

        }

        private void AllPublish(IEvent[] events)
        {
            var seq = _handleQueue.Next(events.Length);
            var index = seq - events.Length + 1;
            var start = index;
            while (index <= seq)
            {
                _handleQueue[index].Value = events[index - start];
                _handleQueue.Publish(index);
                index++;
            }
        }

        private void SplitPublish(IEvent[] events)
        {
            int skip = 0;
            int take = _handleQueue.Size / 2;
            IEvent[] evs = events.Skip(skip).Take(take).ToArray();
            do
            {
                AllPublish(evs);
                skip += evs.Length;
                evs = events.Skip(skip).Take(take).ToArray();

            } while (evs.Length > 0);
        }
    }


}
