using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Command;
using Rainbow.DomainDriven.RingQueue.Event;
using Rainbow.DomainDriven.RingQueue.Utilities;
using System.Linq;
using Rainbow.MessageQueue.Ring;
using Rainbow.DomainDriven.Host;
using Rainbow.DomainDriven.RingQueue.Infrastructure;
using Rainbow.DomainDriven.Event;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Rainbow.DomainDriven.RingQueue.Host
{
    public class LocalQueueDamianHost : DomainHost, IDomainHost
    {
        private readonly IServiceCollection _service;
        private readonly IConfiguration _configuration;
        private readonly IChangeToken _changeToken;

        public LocalQueueDamianHost(IServiceCollection service, IConfiguration configuration)
            : base(service)
        {
            this._service = service;
            this._configuration = configuration;
            this._changeToken = configuration.GetReloadToken();
        }



        public override void Start()
        {
            base.Start();
            this.InitializeQueue();

        }

        private void InitializeQueue()
        {
            var builder = this._provider.GetRequiredService<IMessageProcessBuilder>();

            BuildCommandHander(this._provider, builder);
            BuildEventHandler(this._provider, builder);

            var messageProcess = builder.Build();
            messageProcess.Start();

            var eventSourcingProcess = this._provider.GetService<IEventSourcingProcess>();
            if (eventSourcingProcess != null)
                eventSourcingProcess.Run();
        }

        private void BuildCommandHander(
            IServiceProvider provider
            , IMessageProcessBuilder messageProcessBuilder)
        {

            var queueName = QueueName.CommandQueue;
            var size = this._configuration.GetValue<int>("CommandQueueSize");

            IWaitStrategy wait = new SpinWaitStrategy();
            MultiSequencer sequencer = new MultiSequencer(size, wait);
            RingBuffer<DomainMessage<ICommand>> queue = new RingBuffer<DomainMessage<ICommand>>(sequencer);
            messageProcessBuilder.AddQueue(queueName, queue);

            var barrier = queue.NewBarrier();

            var isCache = provider.GetService<ICommandMappingProvider>() != null;
            if (isCache)
            {
                var cacheHandler = provider.GetService<CommandCacheHandler>();
                IRingBufferConsumer cacheConsumer = new RingBufferConsumer<DomainMessage<ICommand>>(
                    queue,
                    barrier,
                    cacheHandler);

                barrier = queue.NewBarrier(cacheConsumer.Sequence);
                messageProcessBuilder.AddConsumer(queueName, QueueName.CommandCacheConsumer, cacheConsumer);
            }


            var executorHandler = provider.GetRequiredService<CommandExecutorHandler>();
            IRingBufferConsumer executorConsumer = new RingBufferConsumer<DomainMessage<ICommand>>(
                queue,
                barrier,
                executorHandler);

            queue.AddGatingSequences(executorConsumer.Sequence);
            messageProcessBuilder.AddConsumer(queueName, QueueName.CommandExecutorConsumer, executorConsumer);
        }


        private void BuildEventHandler(
            IServiceProvider provider
            , IMessageProcessBuilder messageProcessBuilder)
        {

            var queueName = QueueName.EventQueue;
            var size = this._configuration.GetValue<int>("EventQueueSize");

            IWaitStrategy wait = new SpinWaitStrategy();
            MultiSequencer sequencer = new MultiSequencer(size, wait);

            RingBuffer<DomainMessage<EventStream>> queue = new RingBuffer<DomainMessage<EventStream>>(sequencer);
            messageProcessBuilder.AddQueue(queueName, queue);

            var barrier = queue.NewBarrier();

            var recallHandler = provider.GetRequiredService<EventRecallHandler>();
            IRingBufferConsumer snapshotConsumer = new RingBufferConsumer<DomainMessage<EventStream>>(
                queue,
                barrier,
                recallHandler);

            barrier = queue.NewBarrier(snapshotConsumer.Sequence);

            var executorHandler = provider.GetRequiredService<EventExecutorHandler>();
            IRingBufferConsumer executorConsumer = new RingBufferConsumer<DomainMessage<EventStream>>(
                queue,
                barrier,
                executorHandler);

            queue.AddGatingSequences(executorConsumer.Sequence);

            messageProcessBuilder.AddConsumer(queueName, QueueName.EventSnapshotConsumer, snapshotConsumer);
            messageProcessBuilder.AddConsumer(queueName, QueueName.EventExecutorConsumer, executorConsumer);
        }

    }
}