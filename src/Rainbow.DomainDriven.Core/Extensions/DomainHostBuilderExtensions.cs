using Microsoft.Extensions.DependencyInjection;

namespace Rainbow.DomainDriven
{
    public static class DomainHostBuilderExtensions
    {

        public static IDomainHostBuilder UseDefaultService(this IDomainHostBuilder builder)
        {
            builder.Services.Configure<DomainOptions>(a => a.Add(new DomainServiceExtension()));
            return builder;
        }


        public static IDomainHostBuilder AddOptionExtension(this IDomainHostBuilder builder, IDomainOptionsExtension optionExtension)
        {
            builder.Services.Configure<DomainOptions>(a => a.Add(optionExtension));
            return builder;
        }
    }
}