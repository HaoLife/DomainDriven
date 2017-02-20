using Rainbow.DomainDriven.Message;

namespace Rainbow.DomainDriven.Event
{
    public class EventExecutor : IEventExecutor
    {
        private readonly IEventHandlerProxyProvider _eventHandlerProxyProvider;

        public EventExecutor(IEventHandlerProxyProvider eventHandlerProxyProvider)
        {
            this._eventHandlerProxyProvider = eventHandlerProxyProvider;
        }

        public void Handle(DomainMessage<DomainEventStream> message)
        {
            //这里不做并非处理，因为事件的生成有可能有优先级的如都是User模型中的事件，一个创建，一个修改
            //不能变成修改先执行了，否则会出现逻辑上的错误
            foreach (var item in message.Content.EventSources)
            {
                var proxy = this._eventHandlerProxyProvider.GetEventHanderProxy(item.Event.GetType());
                proxy.Handle(item);
            }
        }
    }
}