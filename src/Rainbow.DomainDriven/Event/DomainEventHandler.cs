using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Core.Utilities;

namespace Rainbow.DomainDriven.Event
{
    public class DomainEventHandler : IEventHandler
    {
        private readonly IEventHandlerSelector _eventHandlerSelector;
        private readonly IEventHandlerActivator _eventHandlerActivator;
        private readonly ILogger _logger;
        private ConcurrentDictionary<Type, Delegate> _cacheInvokes = new ConcurrentDictionary<Type, Delegate>();

        public DomainEventHandler(
            IEventHandlerSelector eventHandlerSelector,
            IEventHandlerActivator eventHandlerActivator,
            ILoggerFactory loggerFactory
            )
        {
            this._eventHandlerSelector = eventHandlerSelector;
            this._eventHandlerActivator = eventHandlerActivator;
            this._logger = loggerFactory.CreateLogger<DomainEventHandler>();
        }

        public void Handle(IEvent evt)
        {
            var func = _cacheInvokes.GetOrAdd(evt.GetType(), GetDelegate);
            try
            {
                func.DynamicInvoke(evt);
            }
            catch (TargetInvocationException tex)
            {
                this._logger.LogError(LogEvent.EventHandle, tex.InnerException, "领域事件执行异常");
            }
            catch (Exception ex)
            {
                this._logger.LogError(LogEvent.EventHandle, ex, "领域事件执行异常");
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