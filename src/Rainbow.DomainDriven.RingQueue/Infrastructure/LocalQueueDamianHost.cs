using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.RingQueue.Command;
using Rainbow.DomainDriven.RingQueue.Event;
using Rainbow.DomainDriven.RingQueue.Queue;
using Rainbow.DomainDriven.RingQueue.Utilities;
using System.Linq;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public class LocalQueueDamianHost : IDomainHost
    {
        private readonly IServiceCollection _service;

        public LocalQueueDamianHost(IServiceCollection service)
        {
            this._service = service;
        }

        public void Start()
        {

            var provider = this._service.BuildServiceProvider();
            var ringQueuqOptions = provider.GetRequiredService<IOptions<RiginQueueOptions>>();
            var builder = provider.GetRequiredService<IMessageProcessBuilder>();

            BuildEventService(this._service, provider, builder, ringQueuqOptions.Value);
            BuildCommandService(this._service, provider, builder, ringQueuqOptions.Value);

            var messageProcess = builder.Build();
            messageProcess.Start();

            var eventSourcingProcess = provider.GetService<IEventSourcingProcess>();
            if (eventSourcingProcess != null)
                eventSourcingProcess.Run();
        }

        private void BuildCommandService(
            IServiceCollection services
            , IServiceProvider provider
            , IMessageProcessBuilder messageProcessBuilder
            , RiginQueueOptions option)
        {
            var queueName = QueueName.CommandQueue;

            MultiSequencer sequencer = new MultiSequencer(option.CommandQueueSize);
            RingQueue<DomainMessage> queue = new RingQueue<DomainMessage>(sequencer);
            messageProcessBuilder.AddQueue(queueName, queue);

            var barrier = queue.NewBarrier();

            var isCache = provider.GetServices<ICommandMappingProvider>().Any();
            if (isCache)
            {
                var cacheHandler = provider.GetService<CommandCacheHandler>();
                IQueueConsumer cacheConsumer = new QueueConsumer<DomainMessage>(
                    queue,
                    barrier,
                    cacheHandler);

                barrier = queue.NewBarrier(cacheConsumer.Sequence);
                messageProcessBuilder.AddConsumer(queueName, QueueName.CommandCacheConsumer, cacheConsumer);
            }


            var executorHandler = provider.GetRequiredService<CommandExecutorHandler>();
            IQueueConsumer executorConsumer = new QueueConsumer<DomainMessage>(
                queue,
                barrier,
                executorHandler);

            queue.AddGatingSequences(executorConsumer.Sequence);
            messageProcessBuilder.AddConsumer(queueName, QueueName.CommandExecutorConsumer, executorConsumer);
        }

        private void BuildEventService(
            IServiceCollection services
            , IServiceProvider provider
            , IMessageProcessBuilder messageProcessBuilder
            , RiginQueueOptions option)
        {
            var queueName = QueueName.EventQueue;

            MultiSequencer sequencer = new MultiSequencer(option.EventQueueSize);

            RingQueue<DomainMessage> queue = new RingQueue<DomainMessage>(sequencer);
            messageProcessBuilder.AddQueue(queueName, queue);

            var barrier = queue.NewBarrier();

            var storeHandler = provider.GetService<EventStoreHandler>();

            IQueueConsumer storeConsumer = new QueueConsumer<DomainMessage>(
                queue,
                barrier,
                storeHandler);

            barrier = queue.NewBarrier(storeConsumer.Sequence);


            var snapshotHandler = provider.GetRequiredService<SnapshotHandler>();
            IQueueConsumer snapshotConsumer = new QueueConsumer<DomainMessage>(
                queue,
                barrier,
                snapshotHandler);

            barrier = queue.NewBarrier(snapshotConsumer.Sequence);

            var executorHandler = provider.GetRequiredService<EventExecutorHandler>();
            IQueueConsumer executorConsumer = new QueueConsumer<DomainMessage>(
                queue,
                barrier,
                executorHandler);

            queue.AddGatingSequences(executorConsumer.Sequence);


            messageProcessBuilder.AddConsumer(queueName, QueueName.EventStoreConsumer, storeConsumer);
            messageProcessBuilder.AddConsumer(queueName, QueueName.EventSnapshotConsumer, snapshotConsumer);
            messageProcessBuilder.AddConsumer(queueName, QueueName.EventExecutorConsumer, executorConsumer);

        }
    }
}