using System;

namespace Rainbow.DomainDriven.Domain
{
    public class ReplayEventProxyProvider : IReplayEventProxyProvider
    {

        private readonly IServiceProvider _serviceProvider;

        public ReplayEventProxyProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public IReplayEventProxy GetReplayEventProxy(Type eventType)
        {
            var genericType = typeof(ReplayEventProxy<>).MakeGenericType(eventType);
            var proxy = this._serviceProvider.GetService(genericType);
            if (proxy != null) return proxy as IReplayEventProxy;

            throw new Exception($"没有找到类型：{eventType.Name}");
        }
    }
}