using Microsoft.Extensions.DependencyInjection;
using Rainbow.DomainDriven.Event;

namespace Rainbow.DomainDriven.Infrastructure
{
    public class EventHandlerInitializeExtension : IDomainInitializeExtension
    {
        public void ApplyServices(IServiceCollection services)
        {

            services.AddSingleton(typeof(EventHandlerProxy<>));
            services.AddSingleton<IEventHandlerProxyProvider, EventHandlerProxyProvider>();
        }
    }
}