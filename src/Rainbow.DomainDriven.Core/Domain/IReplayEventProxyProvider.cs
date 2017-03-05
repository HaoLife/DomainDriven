using System;

namespace Rainbow.DomainDriven.Domain
{
    public interface IReplayEventProxyProvider
    {
         IReplayEventProxy GetReplayEventProxy(Type eventType);
    }
}