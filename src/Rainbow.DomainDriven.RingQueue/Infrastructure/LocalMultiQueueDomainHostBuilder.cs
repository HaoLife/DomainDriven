using Microsoft.Extensions.DependencyInjection;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.RingQueue.Command;

namespace Rainbow.DomainDriven.RingQueue.Infrastructure
{
    public class LocalMultiQueueDomainHostBuilder: IDomainHostBuilder
    {
        private readonly IServiceCollection _service;

        public LocalMultiQueueDomainHostBuilder(IServiceCollection service)
        {
            this._service = service;
        }

        public void Initialize()
        {
            this.AddCore();

            this.Services
                .AddSingleton<ICommandService, RingMultiQueueEventService>();
        }

        public IServiceCollection Services => _service;

        public IDomainHost Build()
        {
            return new LocalMultiQueueDomainHost(_service);
        }

        public void ApplyServices(IDomainInitializeExtension ext)
        {
            ext.ApplyServices(this._service);
        }
    }
}