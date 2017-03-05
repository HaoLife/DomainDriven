using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rainbow.DomainDriven.Core.Utilities;
using Rainbow.DomainDriven.Domain;

namespace Rainbow.DomainDriven.Event
{
    public class EventHandlerProxy<TEvent> : IEventHandlerProxy
        where TEvent : IEvent
    {
        public class HandleParameter
        {
            public Type HandlerType { get; set; }
            public DomainEventSource EventSource { get; set; }
        }

        private readonly IEventHandlerSelector _eventHandlerSelector;
        private readonly IEventHandlerActivator _eventHandlerActivator;
        private readonly ILogger<EventHandlerProxy<TEvent>> _logger;

        public EventHandlerProxy(
            IEventHandlerSelector eventHandlerSelector,
            IEventHandlerActivator eventHandlerActivator,
            ILogger<EventHandlerProxy<TEvent>> logger
            )
        {
            this._eventHandlerSelector = eventHandlerSelector;
            this._eventHandlerActivator = eventHandlerActivator;
            this._logger = logger;
        }

        public void HandlerInvoke(Type type, TEvent evt)
        {
            IEventHandler<TEvent> handler;
            try
            {
                handler = this._eventHandlerActivator.Create<TEvent>(type);
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
                throw new Exception("执行事件错误", ex);
            }
        }

        public void HandlerInvoke(HandleParameter para)
        {
            try
            {
                HandlerInvoke(para.HandlerType, (TEvent)para.EventSource.Event);
            }
            catch (DomainException dex)
            {
                this._logger.LogInformation($"执行事件失败:{dex.Message} - aggr:{para.EventSource.AggregateRootTypeName} id:{para.EventSource.AggregateRootId} version:{para.EventSource.Event.Version}");
            }
            catch (Exception ex)
            {
                this._logger.LogError(LogEvent.Frame, ex, "执行事件过程中发生异常");
            }
        }
        public void Handle(DomainEventSource eventSource)
        {
            var handlerTypes = this._eventHandlerSelector.FindHandlerTypes<TEvent>();
            List<Task> tasks = new List<Task>();
            //并行执行eventHandler，这部分不区分优先级
            foreach (var handleType in handlerTypes)
            {
                Task task = new Task(a => HandlerInvoke((HandleParameter)a), new HandleParameter() { HandlerType = handleType, EventSource = eventSource });
                task.Start();
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}