using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rainbow.DomainDriven.Command;
using System;
using System.IO;

namespace Rainbow.DomainDriven.RingConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddOptions();



            serviceCollection.AddDomain(builder =>
            {
                builder
                    .AddRing(configuration.GetSection("ring"))
                    .AddMongo(configuration.GetSection("mongo"))
                    .AddDomainService();
            });


            var container = new ContainerBuilder();
            container.Populate(serviceCollection);



            var provider = new AutofacServiceProvider(container.Build());

            var commandBus = provider.GetRequiredService<ICommandBus>();
            // commandBus.Publish()

            Console.WriteLine("Hello World!");
        }
    }
}
