using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.RingQueue.Command;
using Rainbow.DomainDriven.RingQueue.Framework;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.DomainDriven.Domain;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Store;
using System.Threading.Tasks;
using Rainbow.DomainDriven.RingQueue.Event;
using Rainbow.DomainDriven.Framework;
using Disruptor;

namespace Rainbow.DomainDriven.RingQueue.Framework
{
    public class RingBufferProcess : IRingBufferProcess
    {
        private IServiceProvider _provider;
        private RingOptions _options;
        private IDisposable _optionsReloadToken;
        private List<IEventProcessor> consumers = new List<IEventProcessor>();

        private RingBuffer<WrapMessage<CommandMessage>> _commandQueue;
        private RingBuffer<WrapMessage<IEvent>> _eventQueue;

        public RingBufferProcess(IServiceProvider provider, IOptions<RingOptions> options)
        {
            _provider = provider;
            _options = options.Value;

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
            var waitStrategy = new SpinWaitWaitStrategy();

            var queue = RingBuffer<WrapMessage<CommandMessage>>.CreateMultiProducer(() => new WrapMessage<CommandMessage>(), size, waitStrategy);
            var barrier = queue.NewBarrier();

            var commandMappingProvider = _provider.GetService<ICommandMappingProvider>();
            if (commandMappingProvider != null)
            {
                var cacheHandler = new RingCommandCacheHandler(
                    commandMappingProvider
                    , rootRebuilder
                    , contextCache
                    , loggerFactory
                    , _options.CommandMaxHandleCount);


                var cacheConsumer = BatchEventProcessorFactory.Create<WrapMessage<CommandMessage>>(queue, barrier, cacheHandler);

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
                , _options.CommandMaxHandleCount
                );

            var executorConsumer = BatchEventProcessorFactory.Create<WrapMessage<CommandMessage>>(queue, barrier, executorHandler);

            consumers.Add(executorConsumer);

            queue.AddGatingSequences(executorConsumer.Sequence);

            _commandQueue = queue;
        }


        private void InitializeEvent()
        {
            var snapshootStoreFactory = _provider.GetRequiredService<ISnapshootStoreFactory>();
            var loggerFactory = _provider.GetRequiredService<ILoggerFactory>();
            var eventHandlerFactory = _provider.GetRequiredService<IEventHandlerFactory>();
            var snapshootCache = _provider.GetRequiredService<ISnapshootCache>();
            var eventRegister = _provider.GetRequiredService<IEventRegister>();
            var assemblyProvider = _provider.GetRequiredService<IAssemblyProvider>();
            var eventRebuildHandler = _provider.GetRequiredService<IEventRebuildHandler>();
            var subscribeEventStore = _provider.GetRequiredService<ISubscribeEventStore>();
            var eventHandleSubject = _provider.GetRequiredService<IEventHandleSubject>();

            var size = _options.EventQueueSize;
            var waitStrategy = new SpinWaitWaitStrategy();

            var queue = RingBuffer<WrapMessage<IEvent>>.CreateMultiProducer(() => new WrapMessage<IEvent>(), size, waitStrategy);

            var barrier = queue.NewBarrier();

            var snapshootHandler = new RingEventSnapshootHandler(
                assemblyProvider
                , snapshootStoreFactory
                , eventRebuildHandler
                , subscribeEventStore
                , snapshootCache
                , loggerFactory
                , eventHandleSubject
                , _options.EventMaxHandleCount);

            var snapshootConsumer = BatchEventProcessorFactory.Create<WrapMessage<IEvent>>(queue, barrier, snapshootHandler);

            barrier = queue.NewBarrier(snapshootConsumer.Sequence);
            consumers.Add(snapshootConsumer);


            var executorHandler = new RingEventBusinessHandler(
                eventRegister
                , eventHandlerFactory
                , subscribeEventStore
                , loggerFactory
                , eventHandleSubject
                , _options.EventMaxHandleCount);

            var executorConsumer = BatchEventProcessorFactory.Create<WrapMessage<IEvent>>(queue, barrier, executorHandler);


            consumers.Add(executorConsumer);

            queue.AddGatingSequences(executorConsumer.Sequence);

            _eventQueue = queue;
        }


        public bool IsStart { get; private set; }

        public RingBuffer<WrapMessage<CommandMessage>> GetCommand()
        {
            return _commandQueue;
        }

        public RingBuffer<WrapMessage<IEvent>> GetEvent()
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
