using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.RingQueue.Command;
using Rainbow.DomainDriven.RingQueue.Framework;
using Rainbow.MessageQueue.Ring;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.DomainDriven.Domain;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Store;
using System.Threading.Tasks;
using Rainbow.DomainDriven.RingQueue.Event;
using Rainbow.DomainDriven.Infrastructure;

namespace Rainbow.DomainDriven.RingQueue.Framework
{
    public class RingBufferProcess : IRingBufferProcess
    {
        private IServiceProvider _provider;
        private RingOptions _options;
        private IDisposable _optionsReloadToken;
        private List<IRingBufferConsumer> consumers = new List<IRingBufferConsumer>();

        private RingBuffer<CommandMessage> _commandQueue;
        private RingBuffer<IEvent> _eventQueue;

        public RingBufferProcess(IServiceProvider provider, IOptionsMonitor<RingOptions> options)
        {
            _provider = provider;
            _optionsReloadToken = options.OnChange(ReloadOptions);
            ReloadOptions(options.CurrentValue);

        }


        private void ReloadOptions(RingOptions options)
        {
            _options = options;
        }


        private void InitializeCommand()
        {
            var rootRebuilder = _provider.GetRequiredService<IAggregateRootRebuilder>();
            var loggerFactory = _provider.GetRequiredService<ILoggerFactory>();
            var commandHandlerFactory = _provider.GetRequiredService<ICommandHandlerFactory>();
            var eventStore = _provider.GetRequiredService<IEventStore>();
            var eventBus = _provider.GetRequiredService<IEventBus>();
            var commandRegister = _provider.GetRequiredService<ICommandRegister>();

            IContextCache contextCache = _provider.GetRequiredService<IContextCache>();

            var size = _options.CommandQueueSize;


            IWaitStrategy wait = new SpinWaitStrategy();
            MultiSequencer sequencer = new MultiSequencer(size, wait);
            RingBuffer<CommandMessage> queue = new RingBuffer<CommandMessage>(sequencer);
            var barrier = queue.NewBarrier();

            var commandMappingProvider = _provider.GetService<ICommandMappingProvider>();
            if (commandMappingProvider != null)
            {
                var cacheHandler = new RingCommandCacheHandler(
                    commandMappingProvider
                    , rootRebuilder
                    , contextCache
                    , loggerFactory);
                IRingBufferConsumer cacheConsumer = new RingBufferConsumer<CommandMessage>(
                    queue,
                    barrier,
                    cacheHandler);

                barrier = queue.NewBarrier(cacheConsumer.Sequence);
                consumers.Add(cacheConsumer);
            }


            var executorHandler = new RingCommandBusinessHandler(
                contextCache
                , commandHandlerFactory
                , eventStore
                , eventBus
                , commandRegister
                , rootRebuilder
                , loggerFactory
                );
            IRingBufferConsumer executorConsumer = new RingBufferConsumer<CommandMessage>(
                queue,
                barrier,
                executorHandler);

            consumers.Add(executorConsumer);

            queue.AddGatingSequences(executorConsumer.Sequence);

            //consumers.ForEach(a => Task.Factory.StartNew(a.Run, TaskCreationOptions.LongRunning));



            _commandQueue = queue;
        }


        private void InitializeEvent()
        {
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

            //consumers.ForEach(a => Task.Factory.StartNew(a.Run, TaskCreationOptions.LongRunning));

            _eventQueue = queue;
        }


        public bool IsStart { get; private set; }

        public RingBuffer<CommandMessage> GetCommand()
        {
            return _commandQueue;
        }

        public RingBuffer<IEvent> GetEvent()
        {
            return _eventQueue;
        }

        public void Start()
        {
            InitializeCommand();
            InitializeEvent();

            consumers.ForEach(a => Task.Factory.StartNew(a.Run, TaskCreationOptions.LongRunning));
            this.IsStart = true;
        }

        public void Stop()
        {

            consumers.ForEach(a => a.Halt());
            consumers.Clear();
            this.IsStart = false;
        }
    }
}
