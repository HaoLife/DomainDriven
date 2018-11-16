using Rainbow.DomainDriven.Event;
using Rainbow.MessageQueue.Ring;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Store;
using Microsoft.Extensions.Caching.Memory;

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class RingEventBusinessHandler : AbstractBatchMessageHandler<IEvent>
    {
        private readonly ConcurrentDictionary<Type, Action<IEvent>> _cache = new ConcurrentDictionary<Type, Action<IEvent>>();
        private static readonly MethodInfo _handleMethod = typeof(RingEventBusinessHandler).GetMethod(nameof(HandleEvent), BindingFlags.Instance | BindingFlags.NonPublic);

        private static Guid _defaultSubscribeId = new Guid("00000000-0000-0000-0000-000000000002");

        private IEventRegister _eventRegister;
        private IEventHandlerFactory _eventHandlerFactory;
        private ILogger<RingEventBusinessHandler> _logger;
        private ISubscribeEventStore _subscribeEventStore;

        private SubscribeEvent _subscribeEvent = new SubscribeEvent() { Id = _defaultSubscribeId, UTCTimestamp = 0 };

        public RingEventBusinessHandler(
            IEventRegister eventRegister
            , IEventHandlerFactory eventHandlerFactory
            , ISubscribeEventStore subscribeEventStore
            , ILoggerFactory loggerFactory)
        {

            _eventRegister = eventRegister;
            _eventHandlerFactory = eventHandlerFactory;
            _subscribeEventStore = subscribeEventStore;
            _logger = loggerFactory.CreateLogger<RingEventBusinessHandler>();


            var subscribeEvent = _subscribeEventStore.Get(_defaultSubscribeId);
            if (subscribeEvent != null) _subscribeEvent = subscribeEvent;
        }


        public override void Handle(IEvent[] messages, long endSequence)
        {
            _logger.LogInformation($"执行事件:{messages.Length}");

            messages = messages.Where(a => a.UTCTimestamp > _subscribeEvent.UTCTimestamp).ToArray();
            if (!messages.Any()) return;

            foreach (var message in messages)
            {
                var call = _cache.GetOrAdd(
                    key: message.GetType(),
                    valueFactory: (type) =>
                    {
                        var getHandleMethod = _handleMethod.MakeGenericMethod(type);
                        var instance = Expression.Constant(this);
                        var parameter = Expression.Parameter(typeof(IEvent), "evt");
                        var expression =
                            Expression.Lambda<Action<IEvent>>(
                                Expression.Call(instance, getHandleMethod, parameter),
                                parameter);
                        return expression.Compile();
                    });

                call(message);
            }
            //记录业务消费者处理的的最后的事件

            var evt = messages.LastOrDefault();
            if (evt != null)
            {
                _subscribeEvent.AggregateRootId = evt.AggregateRootId;
                _subscribeEvent.AggregateRootTypeName = evt.AggregateRootTypeName;
                _subscribeEvent.EventId = evt.Id;
                _subscribeEvent.UTCTimestamp = evt.UTCTimestamp;
                _subscribeEventStore.Save(_subscribeEvent);
            }
        }



        private void HandleEvent<TEvent>(IEvent evt) where TEvent : IEvent
        {
            var types = _eventRegister.FindHandlerType<TEvent>();
            //这里可以启用并行线程了，因为command的等待减少了cpu的使用率
            types.AsParallel().ForAll(a =>
            {
                try
                {
                    _eventHandlerFactory.Create<TEvent>(a).Handle((TEvent)evt);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"执行事件类型[{a.FullName}]异常,事件id:{evt.Id}", ex);
                }
            });
        }
    }
}
