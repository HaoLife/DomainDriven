using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Core.Utilities;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Event
{
    public class EventHandlerProxy : IEventHandlerProxy
    {
        private readonly IEventHandlerSelector _eventHandlerSelector;
        private readonly IEventHandlerActivator _eventHandlerActivator;
        private readonly ILogger _logger;
        private ConcurrentDictionary<Type, Delegate> _cacheInvokes = new ConcurrentDictionary<Type, Delegate>();

        public EventHandlerProxy(
            IEventHandlerSelector eventHandlerSelector,
            IEventHandlerActivator eventHandlerActivator,
            ILogger<EventHandlerProxy> logger
            )
        {
            this._eventHandlerSelector = eventHandlerSelector;
            this._eventHandlerActivator = eventHandlerActivator;
            this._logger = logger;
        }

        public void Handle(IEvent evt)
        {
            var func = _cacheInvokes.GetOrAdd(evt.GetType(), GetDelegate);
            try
            {
                func.DynamicInvoke(evt);
            }
            catch (TargetInvocationException ex)
            {
                this._logger.LogError(LogEvent.Frame, ex.InnerException, "执行事件过程中发生异常");
            }
        }


        private Delegate GetDelegate(Type type)
        {
            var eventTypeExp = Expression.Parameter(type);
            var instance = Expression.Constant(this);
            var methodType = this.GetType().GetMethod(nameof(HandleInvoke),
                BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(type);
            var callExp = Expression.Call(instance, methodType, eventTypeExp);
            return Expression.Lambda(callExp, eventTypeExp).Compile();
        }



        protected virtual void HandleInvoke<TEvent>(TEvent evt) where TEvent : IEvent
        {
            var handlerTypes = this._eventHandlerSelector.FindHandlerTypes<TEvent>();

            foreach (var handleType in handlerTypes)
            {
                IEventHandler<TEvent> handler = null;
                try
                {
                    handler = this._eventHandlerActivator.Create<TEvent>(handleType);
                }
                catch (Exception ex)
                {
                    throw new Exception("事件执行器创建异常", ex);
                }

                try
                {
                    handler.Handle(evt);
                }
                catch (Exception ex)
                {
                    throw new Exception("执行事件异常", ex);
                }
            }
        }
    }
}