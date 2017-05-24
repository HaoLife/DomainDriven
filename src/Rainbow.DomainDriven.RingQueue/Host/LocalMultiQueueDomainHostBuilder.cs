using System;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.DomainExtensions;
using Rainbow.DomainDriven.Host;
using Rainbow.DomainDriven.RingQueue.Command;

namespace Rainbow.DomainDriven.RingQueue.Host
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
                .AddSingleton<ICommandService, RingMultiQueueCommandService>();
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