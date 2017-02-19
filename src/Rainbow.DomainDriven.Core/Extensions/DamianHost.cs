using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Options;

namespace Rainbow.DomainDriven
{
    public class DamianHost : IDomainHost
    {
        private readonly IServiceCollection _service;
        private readonly IServiceProvider _provider;

        public DamianHost(IServiceCollection service, IServiceProvider provider)
        {
            this._service = service;
            this._provider = provider;
        }

        public void Start()
        {
            var options = this._provider.GetService<IOptions<DomainOptions>>();
            foreach (var item in options.Value.Extensions)
            {
                item.ApplyServices(this._service);
            }

        }
    }
}
