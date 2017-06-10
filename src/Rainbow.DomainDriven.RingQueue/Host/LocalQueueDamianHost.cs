using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Rainbow.DomainDriven.Cache;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.Host;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.Repository;
using Rainbow.DomainDriven.RingQueue.Command;
using Rainbow.DomainDriven.RingQueue.Event;
using Rainbow.DomainDriven.RingQueue.Message;
using Rainbow.DomainDriven.RingQueue.Utilities;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Host
{
    public sealed class LocalQueueDamianHost : IDomainHost
    {
        private readonly IConfiguration _configuration;
        private readonly IChangeToken _changeToken;

        private readonly IServiceCollection _services;
        private IServiceProvider _provider;


        //init 
        private ICommandHandler _commandHandler;
        private IEventExecutor _eventExecutor;
        private ICommandExecutorContextFactory _commandExecutorContextFactory;
        private IEventRepoSnapshotHandler _eventRepoSnapshotHandler;
        private IEventReplayHandler _eventReplayHandler;
        private IAggregateRootRepositoryContextFactory _aggregateRootRepositoryContextFactory;
        private IAggregateRootCache _aggregateRootCache;
        private IReplyMessageStore _replyMessageStore;
        private ICommandExecutorFactory _commandExecutorFactory;
        private IDomainTypeProvider _domainTypeProvider;
        private IAssemblyProvider _assemblyProvider;

        //ioc
        private IEventSourceRepository _eventSourceRepository;
        private IMessageProcess _messageProcess;
        private IAggregateRootQuery _aggregateRootQuery;
        private ILoggerFactory _loggerFactory;
        private IEventSourcingRepository _eventSourcingRepository;

        public LocalQueueDamianHost(IServiceCollection services, IConfiguration configuration)
        {
            this._services = services;
            this._configuration = configuration;
            this._changeToken = configuration.GetReloadToken();

        }

        public ICommandExecutorFactory Factory => this._commandExecutorFactory;


        private void InitializeDatabase()
        {
            var initalizer = this._provider?.GetService<IDatabaseInitializer>();
            if (initalizer != null)
            {
                initalizer.Initialize(this._provider);
            }
        }

        public void Start()
        {
            this._provider = this._services.BuildServiceProvider();
            this.InitializeDatabase();
            this.Initialize();
            this.InitializeEventHandler(this._provider, this._messageProcess);
            this.InitializeCommandHander(this._provider, this._messageProcess);
            this._messageProcess.Start();
        }
        private void Initialize()
        {
            this._aggregateRootQuery = this._provider.GetRequiredService<IAggregateRootQuery>();
            this._messageProcess = this._provider.GetRequiredService<IMessageProcess>();
            this._aggregateRootRepositoryContextFactory = this._provider.GetRequiredService<IAggregateRootRepositoryContextFactory>();
            this._eventSourceRepository = this._provider.GetRequiredService<IEventSourceRepository>();
            this._loggerFactory = this._provider.GetRequiredService<ILoggerFactory>();
            this._eventSourcingRepository = this._provider.GetRequiredService<IEventSourcingRepository>();

            this._assemblyProvider = new AssemblyProvider();
            this._aggregateRootCache = new AggregateRootCache();
            this._replyMessageStore = new ReplyMessageStore();
            this._eventReplayHandler = new EventReplayHandler();
            this._domainTypeProvider = new DomainTypeProvider(this._assemblyProvider);
            var commandHandlerActivator = new CommandHandlerActivator(this._provider);
            var commandHandlerSelector = new CommandHandlerSelector(this._assemblyProvider);
            var eventHandlerActivator = new EventHandlerActivator(this._provider);
            var eventHandlerSelector = new EventHandlerSelector(this._assemblyProvider);
            var eventHandler = new DomainEventHandler(eventHandlerSelector, eventHandlerActivator, this._loggerFactory);

            this._commandHandler = new CommandHandler(commandHandlerSelector, commandHandlerActivator);
            this._commandExecutorContextFactory = new MessageCommandExecutorContextFactory(this._aggregateRootQuery, this._aggregateRootCache);
            this._eventExecutor = new EventExecutor(eventHandler); ;
            this._commandExecutorFactory = new MessageExecutorFactory(this._messageProcess, this._replyMessageStore);
            this._eventRepoSnapshotHandler = new EventRepoSnapshotHandler(this._aggregateRootQuery, this._eventSourceRepository, this._eventReplayHandler);


        }


        private void InitializeCommandHander(
            IServiceProvider provider
            , IMessageProcess messageProcess)
        {

            var queueName = QueueName.CommandQueue;
            var size = this._configuration.GetValue<int>("CommandQueueSize");

            IWaitStrategy wait = new SpinWaitStrategy();
            MultiSequencer sequencer = new MultiSequencer(size, wait);
            RingBuffer<DomainMessage<ICommand>> queue = new RingBuffer<DomainMessage<ICommand>>(sequencer);
            messageProcess.AddQueue(queueName, queue);

            var barrier = queue.NewBarrier();

            var commandMappingProvider = provider.GetService<ICommandMappingProvider>();
            if (commandMappingProvider != null)
            {
                var cacheHandler = new MessageCommandCacheHandler(
                    commandMappingProvider
                    , this._aggregateRootQuery
                    , this._aggregateRootCache
                    , this._loggerFactory);
                IRingBufferConsumer cacheConsumer = new RingBufferConsumer<DomainMessage<ICommand>>(
                    queue,
                    barrier,
                    cacheHandler);

                barrier = queue.NewBarrier(cacheConsumer.Sequence);
                messageProcess.AddConsumer(QueueName.CommandCacheConsumer, cacheConsumer);
            }


            var executorHandler = new MessageCommandExecutorHandler(
                this._commandHandler
                , this._commandExecutorContextFactory
                , this._eventRepoSnapshotHandler
                , this._aggregateRootCache
                , this._eventSourceRepository
                , this._messageProcess
                , this._replyMessageStore
                , this._eventReplayHandler);
            IRingBufferConsumer executorConsumer = new RingBufferConsumer<DomainMessage<ICommand>>(
                queue,
                barrier,
                executorHandler);

            queue.AddGatingSequences(executorConsumer.Sequence);
            messageProcess.AddConsumer(QueueName.CommandExecutorConsumer, executorConsumer);
        }


        private void InitializeEventHandler(
            IServiceProvider provider
            , IMessageProcess messageProcessBuilder)
        {

            var queueName = QueueName.EventQueue;
            var size = this._configuration.GetValue<int>("EventQueueSize");

            IWaitStrategy wait = new SpinWaitStrategy();
            MultiSequencer sequencer = new MultiSequencer(size, wait);

            RingBuffer<DomainMessage<EventStream>> queue = new RingBuffer<DomainMessage<EventStream>>(sequencer);
            messageProcessBuilder.AddQueue(queueName, queue);

            var barrier = queue.NewBarrier();
            var recallHandler = new MessageEventRecallHandler(
                this._aggregateRootQuery
                , this._domainTypeProvider
                , this._replyMessageStore
                , this._aggregateRootCache
                , this._aggregateRootRepositoryContextFactory
                , this._loggerFactory
            );
            IRingBufferConsumer recallConsumer = new RingBufferConsumer<DomainMessage<EventStream>>(
                queue,
                barrier,
                recallHandler);

            barrier = queue.NewBarrier(recallConsumer.Sequence);

            var executorHandler = new MessageEventExecutorHandler(
                this._eventExecutor
                , this._eventSourcingRepository
                , this._loggerFactory
            );

            IRingBufferConsumer executorConsumer = new RingBufferConsumer<DomainMessage<EventStream>>(
                queue,
                barrier,
                executorHandler);

            queue.AddGatingSequences(executorConsumer.Sequence);

            messageProcessBuilder.AddConsumer(QueueName.EventRecallConsumer, recallConsumer);
            messageProcessBuilder.AddConsumer(QueueName.EventExecutorConsumer, executorConsumer);
        }

    }
}