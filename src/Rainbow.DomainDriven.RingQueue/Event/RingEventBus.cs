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
            var memoryCache = _provider.GetRequiredService<IMemoryCache>();
            var eventRegister = _provider.GetRequiredService<IEventRegister>();
            var assemblyProvider = _provider.GetRequiredService<IAssemblyProvider>();
            var eventRebuildHandler = _provider.GetRequiredService<IEventRebuildHandler>();
            var subscribeEventStore = _provider.GetRequiredService<ISubscribeEventStore>();

            var size = _options.EventQueueSize;



            IWaitStrategy wait = new SpinWaitStrategy2(_logger);
            MultiSequencer2 sequencer = new MultiSequencer2(size, wait);
            RingBuffer<IEvent> queue = new RingBuffer<IEvent>(sequencer);
            var barrier = queue.NewBarrier();

            var snapshootHandler = new RingEventSnapshootHandler(
                assemblyProvider
                , snapshootStoreFactory
                , eventRebuildHandler
                , subscribeEventStore
                , memoryCache
                , loggerFactory);
            IRingBufferConsumer snapshootConsumer = new RingBufferConsumer2<IEvent>(
                queue,
                barrier,
                snapshootHandler);

            barrier = queue.NewBarrier(snapshootConsumer.Sequence);
            consumers.Add(snapshootConsumer);


            var executorHandler = new RingEventBusinessHandler(
                eventRegister
                , eventHandlerFactory
                , subscribeEventStore
                , loggerFactory);
            IRingBufferConsumer executorConsumer = new RingBufferConsumer3<IEvent>(
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
            _logger.LogInformation($"发送事件:{events.Length}");
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
                _logger.LogDebug($"发送分批:{take} - skip:{skip} - {events.Length}");
                AllPublish(evs);
                skip += evs.Length;
                evs = events.Skip(skip).Take(take).ToArray();

            } while (evs.Length > 0);
        }
    }



    public class SpinWaitStrategy2 : IWaitStrategy
    {
        private ILogger _logger;
        public SpinWaitStrategy2(ILogger logger)
        {
            _logger = logger;
        }

        public long WaitFor(long sequence, ISequence cursor, ISequence dependentSequence, ISequenceBarrier barrier)
        {
            System.Threading.SpinWait spinWait = default(System.Threading.SpinWait);
            long value;
            while ((value = dependentSequence.Value) < sequence)
            {
                barrier.CheckAlert();
                spinWait.SpinOnce();
            }
            _logger.LogDebug($"sequence:{sequence} -{dependentSequence.Value}");
            return value;
        }
    }


    public class MultiSequencer2 : Sequencer
    {

        //队列使用情况标记，每过一轮+1
        private readonly int[] _availableBuffer;
        //队列最大可存储值下标
        private readonly int _indexMask;
        //位移数量值，如1移1位，2移2位，4移3位，8移4位以此类推
        private readonly int _indexShift;
        //消费者最小消费的序列缓存值
        private Sequence _sequenceCache = new Sequence();

        public MultiSequencer2(int bufferSize, IWaitStrategy waitStrategy)
            : base(bufferSize, waitStrategy)
        {
            this._availableBuffer = new int[bufferSize];
            this._indexMask = bufferSize - 1;
            this._indexShift = Util.Log2(bufferSize);
            InitialiseAvailableBuffer();
        }

        #region 私有方法


        private void InitialiseAvailableBuffer()
        {
            for (int i = _availableBuffer.Length - 1; i != 0; i--)
            {
                SetAvailableBufferValue(i, -1);
            }

            SetAvailableBufferValue(0, -1);

        }


        private int CalculateIndex(long sequence)
        {
            return ((int)sequence) & _indexMask;
        }

        private int CalculateAvailabilityFlag(long sequence)
        {
            return (int)((ulong)sequence >> _indexShift);
        }

        private void SetAvailableBufferValue(int index, int flag)
        {
            _availableBuffer[index] = flag;
        }

        private void SetAvailable(long sequence)
        {
            SetAvailableBufferValue(CalculateIndex(sequence), CalculateAvailabilityFlag(sequence));
        }

        private bool IsAvailable(long sequence)
        {
            int index = CalculateIndex(sequence);
            int flag = CalculateAvailabilityFlag(sequence);
            return Volatile.Read(ref _availableBuffer[index]) == flag;
        }
        #endregion

        public override long Next()
        {
            return Next(1);
        }

        public override long Next(int n)
        {

            if (n < 1)
            {
                throw new ArgumentException("n must be > 0");
            }

            long current;
            long next;

            var spinWait = new SpinWait();
            do
            {
                current = _sequence.Value;
                next = current + n;

                long offsetSequence = next - _bufferSize;
                long cachedGatingSequence = _sequenceCache.Value;

                //获取队列长度与当前处理值比较，如果没有超过，为可用
                if (offsetSequence > cachedGatingSequence || cachedGatingSequence > current)
                {
                    //询问消费者已经处理的最小的序列是多少，并进行设置
                    long gatingSequence = Util.GetMinimum(this._gatingSequences, current);

                    if (offsetSequence > gatingSequence)
                    {
                        spinWait.SpinOnce();
                        continue;
                    }

                    _sequenceCache.SetValue(gatingSequence);
                }
                else if (_sequence.CompareAndSet(current, next))
                {
                    break;
                }
            }
            while (true);

            return next;
        }

        public override void Publish(long sequence)
        {
            SetAvailable(sequence);
        }

        public override void Publish(long lo, long hi)
        {
            for (long l = lo; l <= hi; l++)
            {
                SetAvailable(l);
            }
        }

        public override long GetAvailableSequence(long lo, long hi)
        {
            for (long sequence = lo; sequence <= hi; sequence++)
            {
                if (!IsAvailable(sequence))
                {
                    return sequence - 1;
                }
            }

            return hi;
        }

    }


    public class RingBufferConsumer2<TMessage> : IRingBufferConsumer
    {

        private volatile int _running;
        private readonly IRingBuffer<TMessage> _messageBuffer;
        private readonly ISequenceBarrier _sequenceBarrier;
        private readonly IMessageHandler<TMessage> _batchMessageHandler;
        private Sequence _current = new Sequence();

        public RingBufferConsumer2(
            IRingBuffer<TMessage> messageQueue,
            ISequenceBarrier sequenceBarrier,
            IMessageHandler<TMessage> batchMessageHandler)
        {
            this._messageBuffer = messageQueue;
            this._sequenceBarrier = sequenceBarrier;
            this._batchMessageHandler = batchMessageHandler;
        }

        public Sequence Sequence => _current;

        public bool IsRunning => this._running == 1;

        public void Halt()
        {
            _running = 0;
            _sequenceBarrier.Alert();
        }
        public void Run()
        {
            if (Interlocked.Exchange(ref _running, 1) != 0)
            {
                throw new InvalidOperationException("Thread is already running");
            }

            _sequenceBarrier.ClearAlert();
            var nextSequence = _current.Value + 1L;

            while (true)
            {
                try
                {
                    var availableSequence = _sequenceBarrier.WaitFor(nextSequence);

                    while (nextSequence <= availableSequence)
                    {
                        this._batchMessageHandler.Handle(_messageBuffer[nextSequence].Value, nextSequence, nextSequence == availableSequence);
                        nextSequence++;
                    }

                    _current.SetValue(availableSequence);
                }
                catch (AlertException)
                {
                    if (_running == 0)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _current.SetValue(nextSequence);
                    nextSequence++;
                }
            }
        }
    }

    public class RingBufferConsumer3<TMessage> : IRingBufferConsumer
    {

        private volatile int _running;
        private readonly IRingBuffer<TMessage> _messageBuffer;
        private readonly ISequenceBarrier _sequenceBarrier;
        private readonly IMessageHandler<TMessage> _batchMessageHandler;
        private Sequence _current = new Sequence();

        public RingBufferConsumer3(
            IRingBuffer<TMessage> messageQueue,
            ISequenceBarrier sequenceBarrier,
            IMessageHandler<TMessage> batchMessageHandler)
        {
            this._messageBuffer = messageQueue;
            this._sequenceBarrier = sequenceBarrier;
            this._batchMessageHandler = batchMessageHandler;
        }

        public Sequence Sequence => _current;

        public bool IsRunning => this._running == 1;

        public void Halt()
        {
            _running = 0;
            _sequenceBarrier.Alert();
        }
        public void Run()
        {
            if (Interlocked.Exchange(ref _running, 1) != 0)
            {
                throw new InvalidOperationException("Thread is already running");
            }

            _sequenceBarrier.ClearAlert();
            var nextSequence = _current.Value + 1L;

            while (true)
            {
                try
                {
                    var availableSequence = _sequenceBarrier.WaitFor(nextSequence);

                    while (nextSequence <= availableSequence)
                    {
                        this._batchMessageHandler.Handle(_messageBuffer[nextSequence].Value, nextSequence, nextSequence == availableSequence);
                        nextSequence++;
                    }

                    _current.SetValue(availableSequence);
                }
                catch (AlertException)
                {
                    if (_running == 0)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _current.SetValue(nextSequence);
                    nextSequence++;
                }
            }
        }
    }
}
