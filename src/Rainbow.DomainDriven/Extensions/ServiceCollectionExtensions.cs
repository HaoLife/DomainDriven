
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rainbow.DomainDriven.Command;
using Rainbow.DomainDriven.Infrastructure;
using Rainbow.DomainDriven.Host;
using Rainbow.DomainDriven.Cache;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.TryAdd(new ServiceCollection()
                // .AddSingleton<IAssemblyProvider, AssemblyProvider>()
                // .AddSingleton<IDomainTypeProvider, DomainTypeProvider>()
                // .AddSingleton<ICommandExecutorFactory, ObjectExecutorFactory>()
                .AddSingleton<ICommandService, CommandService>()
                .AddSingleton<IDomainHost>(new DomainHost(services))
            );
            return services;
        }


        public static IServiceCollection AddDomainService(this IServiceCollection services)
        {
            var builder = new DomainServiceBuilder(services);
            builder.Build();
            return services;
        }


        public static IServiceCollection AddCommandMapping<TCommandMappingProvider>(this IServiceCollection services)
            where TCommandMappingProvider : class, ICommandMappingProvider
        {

            services.TryAdd(new ServiceCollection()
                .AddSingleton<ICommandMappingProvider, TCommandMappingProvider>()
            );
            
            return services;
        }

    }
}