using Microsoft.Extensions.Options;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.RingQueue.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.MessageQueue.Ring;
using Rainbow.DomainDriven.Domain;
using Microsoft.Extensions.Logging;
using System.Linq;
using Rainbow.DomainDriven.Store;
using Rainbow.DomainDriven.Event;
using System.Threading;

namespace Rainbow.DomainDriven.RingQueue.Command
{
    public class RingCommandBus : ICommandBus
    {
        private IServiceProvider _provider;
        private RingOptions _options;
        private IDisposable _optionsReloadToken;
        private List<IRingBufferConsumer> consumers = new List<IRingBufferConsumer>();
        private RingBuffer<ICommand> _handleQueue;
        private RingBuffer<ReplyMessage> _replyQueue;
        private SingleSequencer _replySequencer;


        public RingCommandBus(IOptionsMonitor<RingOptions> options, IServiceProvider provider)
        {
            provider.GetRequiredService<IOptionsMonitor<RingOptions>>();

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

            var rootRebuilder = _provider.GetRequiredService<IAggregateRootRebuilder>();
            var loggerFactory = _provider.GetRequiredService<ILoggerFactory>();
            var commandHandlerFactory = _provider.GetRequiredService<ICommandHandlerFactory>();
            var eventStore = _provider.GetRequiredService<IEventStore>();
            var eventBus = _provider.GetRequiredService<IEventBus>();
            var commandRegister = _provider.GetRequiredService<ICommandRegister>();
            var aggregateRootRebuilder = _provider.GetRequiredService<IAggregateRootRebuilder>();

            IContextCache contextCache = new RingContextCache();

            var size = _options.CommandQueueSize;


            IWaitStrategy replyWait = new SpinWaitStrategy();
            SingleSequencer replySequencer = new SingleSequencer(size, replyWait);
            RingBuffer<ReplyMessage> replyQueue = new RingBuffer<ReplyMessage>(replySequencer);
            var replyBus = new SequenceReplyBus(replyQueue);


            IWaitStrategy wait = new SpinWaitStrategy();
            MultiSequencer sequencer = new MultiSequencer(size, wait);
            RingBuffer<ICommand> queue = new RingBuffer<ICommand>(sequencer);
            var barrier = queue.NewBarrier();
            List<Sequence> sequences = new List<Sequence>();

            var commandMappingProvider = _provider.GetService<ICommandMappingProvider>();
            if (commandMappingProvider != null)
            {
                var cacheHandler = new RingCommandCacheHandler(
                    commandMappingProvider
                    , rootRebuilder
                    , contextCache
                    , loggerFactory);
                IRingBufferConsumer cacheConsumer = new RingBufferConsumer<ICommand>(
                    queue,
                    barrier,
                    cacheHandler);

                barrier = queue.NewBarrier(cacheConsumer.Sequence);
                sequences.Add(cacheConsumer.Sequence);
                consumers.Add(cacheConsumer);
            }


            var executorHandler = new RingCommandBusinessHandler(
                contextCache
                , commandHandlerFactory
                , eventStore
                , eventBus
                , replyBus
                , commandRegister
                , aggregateRootRebuilder
                );
            IRingBufferConsumer executorConsumer = new RingBufferConsumer<ICommand>(
                queue,
                barrier,
                executorHandler);

            sequences.Add(executorConsumer.Sequence);
            consumers.Add(executorConsumer);

            queue.AddGatingSequences(sequences.ToArray());

            consumers.ForEach(a => Task.Factory.StartNew(a.Run, TaskCreationOptions.LongRunning));



            _handleQueue = queue;
            _replyQueue = replyQueue;
            _replySequencer = replySequencer;
        }


        public Task Publish(ICommand command)
        {
            var index = _handleQueue.Next();
            _handleQueue[index].Value = command;
            _handleQueue.Publish(index);

            return Task.Factory.StartNew(() =>
            {
                while (_replySequencer.Current < index)
                {
                    Thread.Sleep(0);
                }
            });
        }
    }
}
