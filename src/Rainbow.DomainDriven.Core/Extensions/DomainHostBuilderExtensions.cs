using Microsoft.Extensions.DependencyInjection;
using Rainbow.DomainDriven.DomainExtensions;
using Rainbow.DomainDriven.Host;

namespace Rainbow.DomainDriven
{
    public static class DomainHostBuilderExtensions
    {

        public static IDomainHostBuilder UseDefaultService(this IDomainHostBuilder builder)
        {
            builder.ApplyServices(new DomainServiceInitializeExtension());
            return builder;
        }


        public static IDomainHostBuilder AddOptionExtension(this IDomainHostBuilder builder, IDomainInitializeExtension optionExtension)
        {
            builder.ApplyServices(optionExtension);
            return builder;
        }
    }
}