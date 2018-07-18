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

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class RingEventBus : IEventBus
    {
        private IServiceProvider _provider;
        private RingOptions _options;
        private IDisposable _optionsReloadToken;
        private List<IRingBufferConsumer> consumers = new List<IRingBufferConsumer>();
        private RingBuffer<IEvent> _handleQueue;

        public RingEventBus(IOptionsMonitor<RingOptions> options, IServiceProvider provider)
        {
            _provider = provider;
            _optionsReloadToken = options.OnChange(ReloadOptions);
            ReloadOptions(options.CurrentValue);
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
            var commandHandlerFactory = _provider.GetRequiredService<IEventHandlerFactory>();
            var eventStore = _provider.GetRequiredService<IMemoryCache>();
            var eventRegister = _provider.GetRequiredService<IEventRegister>();
            var assemblyProvider = _provider.GetRequiredService<IAssemblyProvider>();

            var size = _options.EventQueueSize;



            IWaitStrategy wait = new SpinWaitStrategy();
            MultiSequencer sequencer = new MultiSequencer(size, wait);
            RingBuffer<IEvent> queue = new RingBuffer<IEvent>(sequencer);
            var barrier = queue.NewBarrier();

            var snapshootHandler = new RingEventSnapshootHandler(
                assemblyProvider);
            IRingBufferConsumer snapshootConsumer = new RingBufferConsumer<IEvent>(
                queue,
                barrier,
                snapshootHandler);

            barrier = queue.NewBarrier(snapshootConsumer.Sequence);
            consumers.Add(snapshootConsumer);


            var executorHandler = new RingEventBusinessHandler();
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
            var seq = _handleQueue.Next(events.Length);
            var index = seq - events.Length + 1;
            while (index <= seq)
            {
                _handleQueue.Publish(index);
                index++;
            }

        }
    }
}
