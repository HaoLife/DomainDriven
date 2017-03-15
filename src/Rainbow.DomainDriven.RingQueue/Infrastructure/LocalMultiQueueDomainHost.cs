using System;
using System.Collections.Generic;
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
    public class LocalMultiQueueDomainHost : IDomainHost
    {
        private readonly IServiceCollection _service;

        public LocalMultiQueueDomainHost(IServiceCollection service)
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

            int[] sizes = new int[] {
                option.CommandQueueSize >> 2,
                option.CommandQueueSize >> 2,
                option.CommandQueueSize >> 1
                };

            RingQueue<DomainMessage>[] queues = new RingQueue<DomainMessage>[3];
            ISequenceBarrier[] barriers = new ISequenceBarrier[3];
            Sequence[] seqs = new Sequence[3];


            for (int i = 0; i < sizes.Length; i++)
            {
                var multiQueueName = $"{queueName}:{i}";
                MultiSequencer sequencer = new MultiSequencer(sizes[i]);
                RingQueue<DomainMessage> queue = new RingQueue<DomainMessage>(sequencer);
                messageProcessBuilder.AddQueue(multiQueueName, queue);
                queues[i] = queue;
                barriers[i] = queue.NewBarrier();
            }

            var isCache = provider.GetServices<ICommandMappingProvider>().Any();
            if (isCache)
            {
                var cacheHandler = provider.GetService<CommandCacheHandler>();
                MultiQueueConsumer<DomainMessage> cacheConsumer = new MultiQueueConsumer<DomainMessage>(
                    queues,
                    barriers.ToArray(),
                    cacheHandler);

                seqs = cacheConsumer.GetSequences();
                for (int i = 0; i < seqs.Length; i++)
                {
                    barriers[i] = queues[i].NewBarrier(seqs[i]);
                }

                messageProcessBuilder.AddConsumer(queueName, QueueName.CommandCacheConsumer, cacheConsumer);
            }


            var executorHandler = provider.GetRequiredService<CommandExecutorHandler>();
            MultiQueueConsumer<DomainMessage> executorConsumer = new MultiQueueConsumer<DomainMessage>(
                queues,
                barriers.ToArray(),
                executorHandler);

            seqs = executorConsumer.GetSequences();

            for (int i = 0; i < seqs.Length; i++)
            {
                queues[i].AddGatingSequences(seqs[i]);
            }
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