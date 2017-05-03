using System;
using System.Linq;
using System.Collections.Generic;
using Rainbow.DomainDriven.Domain;
using Rainbow.DomainDriven.Message;
using Rainbow.DomainDriven.Repository;
using Rainbow.DomainDriven.Event;
using Rainbow.DomainDriven.RingQueue.Message;
using Rainbow.DomainDriven.RingQueue.Utilities;
using Rainbow.MessageQueue.Ring;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public class RollbackService : IRollbackService
    {
        private readonly IRingBuffer<DomainMessage> _messageQueue;
        private readonly ISequenceBarrier _sequenceBarrier;
        private readonly Sequence _sequence;
        private readonly IAggregateRootCommonQueryRepository _aggregateRootCommonQueryRepository;
        private readonly IReplayEventProxyProvider _replayEventProxyProvider;

        public RollbackService(
            IMessageProcessBuilder messageProcessBuilder
            , IAggregateRootCommonQueryRepository aggregateRootCommonQueryRepository
            , IReplayEventProxyProvider replayEventProxyProvider
            )
        {
            var process = messageProcessBuilder.Build();
            this._messageQueue = process.GetQueue(QueueName.EventQueue);
            this._sequenceBarrier = this._messageQueue.NewBarrier();
            this._sequence = process.GetConsumer(QueueName.EventQueue, QueueName.EventStoreConsumer)?.Sequence;
            this._aggregateRootCommonQueryRepository = aggregateRootCommonQueryRepository;
            this._replayEventProxyProvider = replayEventProxyProvider;
        }

        public IEnumerable<IAggregateRoot> Redo(IEnumerable<IAggregateRoot> roots)
        {
            List<IAggregateRoot> currents = new List<IAggregateRoot>();
            if (!roots.Any()) return currents;
            Dictionary<string, List<DomainEventSource>> eventSources = new Dictionary<string, List<DomainEventSource>>();

            var nextSequence = _sequence.Value + 1L;
            var availableSequence = _sequenceBarrier.WaitFor(nextSequence);
            while (nextSequence <= availableSequence)
            {
                var evt = _messageQueue[nextSequence];
                var stream = evt.Value.Content as DomainEventStream;
                nextSequence++;
                if (stream == null) continue;
                foreach (var item in stream.EventSources)
                {
                    var aggr = roots.First(a => a.Id == item.AggregateRootId && a.GetType().Name == item.AggregateRootTypeName);
                    if (aggr != null)
                    {
                        if (!eventSources.ContainsKey(item.AggregateRootTypeName)) eventSources.Add(item.AggregateRootTypeName, new List<DomainEventSource>());
                        eventSources[item.AggregateRootTypeName].Add(item);
                    }
                }
            }

            foreach (var item in roots)
            {
                var root = this._aggregateRootCommonQueryRepository.Get(item.GetType(), item.Id);
                currents.Add(root);
                if (!eventSources.ContainsKey(root.GetType().Name)) continue;
                var sources = eventSources[root.GetType().Name].Where(a => a.AggregateRootId == root.Id).ToList();
                foreach (var source in sources)
                {
                    var replayEventProxy = this._replayEventProxyProvider.GetReplayEventProxy(source.Event.GetType());
                    replayEventProxy.Handle(root, source.Event);
                }
            }
            return currents;
        }
    }
}