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

namespace Rainbow.DomainDriven.RingQueue.Event
{
    public class RingEventBusinessHandler : IMessageHandler<IEvent>
    {
        private readonly ConcurrentDictionary<Type, Action<IEvent>> _cache = new ConcurrentDictionary<Type, Action<IEvent>>();
        private static readonly MethodInfo _handleCommandMethod = typeof(RingEventBusinessHandler).GetMethod(nameof(HandleEvent), BindingFlags.Instance | BindingFlags.NonPublic);

        private IEventRegister _eventRegister;
        private IEventHandlerFactory _eventHandlerFactory;
        private ILogger<RingEventBusinessHandler> _logger;

        public void Handle(IEvent[] messages)
        {
            foreach (var message in messages)
            {
                var call = _cache.GetOrAdd(
                    key: message.GetType(),
                    valueFactory: (type) =>
                    {
                        var getHandleMethod = _handleCommandMethod.MakeGenericMethod(type);
                        var parameter = Expression.Parameter(typeof(IEvent), "evt");
                        var expression =
                            Expression.Lambda<Action<IEvent>>(
                                Expression.Call(null, getHandleMethod, parameter),
                                parameter);
                        return expression.Compile();
                    });

                call(message);
            }
            //记录业务消费者处理的的最后的事件
        }



        private void HandleEvent<TEvent>(IEvent evt) where TEvent : IEvent
        {
            var types = _eventRegister.FindHandlerType<TEvent>();
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
