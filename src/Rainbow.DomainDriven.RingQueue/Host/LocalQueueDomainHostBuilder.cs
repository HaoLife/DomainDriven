using Microsoft.Extensions.DependencyInjection;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.DomainExtensions;
using Rainbow.DomainDriven.Host;
using Rainbow.DomainDriven.RingQueue.Command;

namespace Rainbow.DomainDriven.RingQueue.Host
{
    public class LocalQueueDomainHostBuilder : IDomainHostBuilder
    {
        private readonly IServiceCollection _service;

        public LocalQueueDomainHostBuilder(IServiceCollection service)
        {
            this._service = service;
        }

        public void Initialize()
        {
            this.AddCore();

            this.Services
                .AddSingleton<ICommandService, RingQueueCommandService>();
        }

        public IServiceCollection Services => _service;

        public IDomainHost Build()
        {
            return new LocalQueueDamianHost(_service);
        }

        public void ApplyServices(IDomainInitializeExtension ext)
        {
            ext.ApplyServices(this._service);
        }
    }
}