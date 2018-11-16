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
        private RingBuffer<CommandMessage> _handleQueue;
        private ILogger _logger;


        public RingCommandBus(IOptionsMonitor<RingOptions> options, IServiceProvider provider)
        {
            _provider = provider;
            _optionsReloadToken = options.OnChange(ReloadOptions);
            ReloadOptions(options.CurrentValue);
            _logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger<RingCommandBus>();
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

            IContextCache contextCache = new RingContextCache();

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

            consumers.ForEach(a => Task.Factory.StartNew(a.Run, TaskCreationOptions.LongRunning));



            _handleQueue = queue;
        }


        public Task Publish(ICommand command)
        {
            var msg = new CommandMessage(command);
            var index = _handleQueue.Next();
            _handleQueue[index].Value = msg;
            _handleQueue.Publish(index);

            return Task.Factory.StartNew(() =>
            {
                msg.Notice.WaitOne();
                _logger.LogDebug($"等待完成:{index} - {command.Id}");
                ReplyMessage message = msg.Reply;
                if (message.CommandId == command.Id)
                {
                    if (!message.IsSuccess) throw message.Exception;
                }
            });
        }
    }
}
